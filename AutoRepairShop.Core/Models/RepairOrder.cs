using System.Text.Json.Serialization;
using AutoRepairShop.Core.Exceptions;
using AutoRepairShop.Core.Interfaces;
using AutoRepairShop.Core.Validation;

namespace AutoRepairShop.Core.Models;

/// <summary>
/// Замовлення на ремонт (наряд-замовлення). Містить роботи та запчастини і рахує сумарну вартість.
/// Статус змінюється лише методами класу: New → InProgress → Completed або → Cancelled.
/// </summary>
public class RepairOrder : EntityBase, ICostCalculable
{
    private int _carId;
    private string _problemDescription = string.Empty;

    [JsonConstructor]
    private RepairOrder()
    {
    }

    public RepairOrder(int carId, string problemDescription, DateTime createdAt)
    {
        CarId = carId;
        ProblemDescription = problemDescription;
        CreatedAt = createdAt;
        Status = OrderStatus.New;
    }

    [JsonIgnore]
    public override string EntityName => "Замовлення";

    [JsonInclude]
    public int CarId
    {
        get => _carId;
        private set => _carId = Guard.Range(value, "ID автомобіля", 1, int.MaxValue);
    }

    public string ProblemDescription
    {
        get => _problemDescription;
        set => _problemDescription = Guard.Text(value, "Опис несправності", 3, 200);
    }

    [JsonInclude]
    public int? MechanicId { get; private set; }

    [JsonInclude]
    public OrderStatus Status { get; private set; }

    [JsonInclude]
    public DateTime CreatedAt { get; private set; }

    [JsonInclude]
    public DateTime? ClosedAt { get; private set; }

    /// <summary>Списки зберігаються в JSON, але ззовні доступні лише для читання.</summary>
    [JsonInclude, JsonPropertyName("Works")]
    private List<RepairWork> WorkItems { get; set; } = new();

    [JsonInclude, JsonPropertyName("Parts")]
    private List<PartUsage> PartItems { get; set; } = new();

    [JsonIgnore]
    public IReadOnlyList<RepairWork> Works => WorkItems;

    [JsonIgnore]
    public IReadOnlyList<PartUsage> Parts => PartItems;

    /// <summary>Усі рядки чека разом — список базового типу OrderLine.</summary>
    [JsonIgnore]
    public IEnumerable<OrderLine> Lines => WorkItems.Cast<OrderLine>().Concat(PartItems);

    [JsonIgnore]
    public bool IsActive => Status is OrderStatus.New or OrderStatus.InProgress;

    [JsonIgnore]
    public decimal LaborCost => WorkItems.Sum(work => work.CalculateCost());

    [JsonIgnore]
    public decimal PartsCost => PartItems.Sum(part => part.CalculateCost());

    [JsonIgnore]
    public decimal TotalHours => WorkItems.Sum(work => work.Hours);

    /// <summary>Сумарна вартість ремонтних робіт і використаних запчастин.</summary>
    public decimal CalculateCost() => LaborCost + PartsCost;

    /// <summary>Призначити (або змінити) механіка. Перевірку зайнятості робить сервіс.</summary>
    public void AssignMechanic(Mechanic mechanic)
    {
        ArgumentNullException.ThrowIfNull(mechanic);
        EnsureActive("призначити механіка");
        MechanicId = mechanic.Id;
        Status = OrderStatus.InProgress;
    }

    /// <summary>Додати роботу; потрібен призначений механік, бо від нього береться ставка.</summary>
    public RepairWork AddWork(string description, decimal hours, decimal hourlyRate)
    {
        if (Status != OrderStatus.InProgress)
            throw new InvalidOrderStateException(Id, Status, "додати роботу (спочатку призначте механіка)");
        var work = new RepairWork(description, hours, hourlyRate);
        WorkItems.Add(work);
        return work;
    }

    /// <summary>Списати запчастину зі складу та додати в замовлення.</summary>
    public PartUsage AddPart(SparePart part, int quantity)
    {
        ArgumentNullException.ThrowIfNull(part);
        EnsureActive("додати запчастину");
        part.Take(quantity); // PartOutOfStockException, якщо не вистачає

        int index = PartItems.FindIndex(item => item.PartId == part.Id && item.UnitPrice == part.Price);
        PartUsage usage = index >= 0
            ? PartItems[index].WithAddedQuantity(quantity)
            : new PartUsage(part.Id, part.PartNumber, part.Name, quantity, part.Price);
        if (index >= 0)
            PartItems[index] = usage;
        else
            PartItems.Add(usage);
        return usage;
    }

    /// <summary>Завершити ремонт: має бути хоча б одна виконана робота.</summary>
    public void Complete(DateTime closedAt)
    {
        if (Status != OrderStatus.InProgress)
            throw new InvalidOrderStateException(Id, Status, "завершити ремонт");
        if (WorkItems.Count == 0)
            throw new InvalidOrderStateException(Id, Status, "завершити замовлення без жодної виконаної роботи");
        Status = OrderStatus.Completed;
        ClosedAt = closedAt;
    }

    /// <summary>Скасувати замовлення. Повертає запчастини, які треба повернути на склад.</summary>
    public IReadOnlyList<PartUsage> Cancel(DateTime closedAt)
    {
        EnsureActive("скасувати");
        Status = OrderStatus.Cancelled;
        ClosedAt = closedAt;
        return PartItems.ToList();
    }

    public override string GetSummary() =>
        $"Замовлення №{Id}: {ProblemDescription} [{Status.ToUkrainian()}] — {CalculateCost():0.00} грн";

    public override void Validate()
    {
        CarId = _carId;
        ProblemDescription = _problemDescription;
        Guard.Defined(Status, "Статус");
        if (Status is OrderStatus.InProgress or OrderStatus.Completed && MechanicId is null)
            throw new ValidationException("Механік", $"Замовлення №{Id}: статус «{Status.ToUkrainian()}», але механіка не призначено.");
        if (IsActive == ClosedAt.HasValue)
            throw new ValidationException("Дата закриття", $"Замовлення №{Id}: дата закриття не відповідає статусу.");
    }

    private void EnsureActive(string action)
    {
        if (!IsActive)
            throw new InvalidOrderStateException(Id, Status, action);
    }
}
