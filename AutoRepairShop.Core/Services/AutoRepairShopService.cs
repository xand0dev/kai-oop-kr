using AutoRepairShop.Core.Comparers;
using AutoRepairShop.Core.Exceptions;
using AutoRepairShop.Core.Interfaces;
using AutoRepairShop.Core.Models;
using AutoRepairShop.Core.Repositories;
using AutoRepairShop.Core.Storage;

namespace AutoRepairShop.Core.Services;

/// <summary>
/// Фасад бізнес-логіки СТО: облік автомобілів, механіків, складу запчастин і замовлень на ремонт.
/// Інтерфейс користувача працює лише через цей клас.
/// </summary>
public class AutoRepairShopService
{
    private readonly IDataStorage _storage;
    private readonly Func<DateTime> _clock;
    private readonly IRepository<Car> _cars = new Repository<Car>("Автомобіль");
    private readonly IRepository<Mechanic> _mechanics = new Repository<Mechanic>("Механік");
    private readonly IRepository<SparePart> _parts = new Repository<SparePart>("Запчастина");
    private readonly IRepository<RepairOrder> _orders = new Repository<RepairOrder>("Замовлення");

    public AutoRepairShopService(IDataStorage storage, Func<DateTime>? clock = null)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _clock = clock ?? (() => DateTime.Now);
    }

    /// <summary>Чи є зміни, які ще не записані у файл.</summary>
    public bool HasUnsavedChanges { get; private set; }

    public string StorageLocation => _storage.Location;

    public IReadOnlyList<Car> Cars => _cars.GetAll();
    public IReadOnlyList<Mechanic> Mechanics => _mechanics.GetAll();
    public IReadOnlyList<SparePart> SpareParts => _parts.GetAll();
    public IReadOnlyList<RepairOrder> Orders => _orders.GetAll();

    // ---------------------------------------------------------------- файл

    /// <summary>Завантажити дані з файла; повертає false, якщо файла ще немає.</summary>
    public bool Load()
    {
        if (!_storage.Exists)
            return false;
        ShopData data = _storage.Load();
        try
        {
            Apply(data);
        }
        catch (AutoRepairShopException ex)
        {
            ClearAll();
            throw new DataStorageException("Файл містить некоректні дані", _storage.Location, ex);
        }

        HasUnsavedChanges = false;
        return true;
    }

    public void Save()
    {
        var data = new ShopData
        {
            SavedAt = _clock(),
            Cars = _cars.GetAll().ToList(),
            Mechanics = _mechanics.GetAll().ToList(),
            SpareParts = _parts.GetAll().ToList(),
            Orders = _orders.GetAll().ToList(),
        };
        _storage.Save(data);
        HasUnsavedChanges = false;
    }

    /// <summary>Замінити поточні дані демонстраційним набором.</summary>
    public void LoadDemoData()
    {
        ClearAll();
        DemoData.Fill(this);
        HasUnsavedChanges = true;
    }

    // ---------------------------------------------------------- автомобілі

    public Car AddCar(Car car)
    {
        ArgumentNullException.ThrowIfNull(car);
        if (_cars.Exists(c => c.LicensePlate == car.LicensePlate))
            throw new DuplicateEntityException("Автомобіль", car.LicensePlate);
        _cars.Add(car);
        MarkChanged();
        return car;
    }

    public Car GetCar(int id) => _cars[id];

    /// <summary>Оновити дані власника та пробіг (пробіг не може зменшуватися).</summary>
    public void UpdateCar(int id, string ownerName, string ownerPhone, int mileageKm)
    {
        Car car = _cars.GetById(id);
        // Перевіряємо всі значення до зміни, щоб помилка не залишила об'єкт напівзміненим.
        var probe = new Car(car.LicensePlate, car.Make, car.Model, car.Year, ownerName, ownerPhone, car.MileageKm);
        probe.MileageKm = mileageKm;
        car.OwnerName = probe.OwnerName;
        car.OwnerPhone = probe.OwnerPhone;
        car.MileageKm = probe.MileageKm;
        MarkChanged();
    }

    public void RemoveCar(int id)
    {
        Car car = _cars.GetById(id);
        if (_orders.Exists(o => o.CarId == id))
            throw new EntityInUseException(car.EntityName, id, "для нього є замовлення на ремонт");
        _cars.Remove(id);
        MarkChanged();
    }

    /// <summary>Пошук за частиною держномера, марки або імені власника.</summary>
    public List<Car> FindCars(string query)
    {
        string text = (query ?? string.Empty).Trim();
        return _cars.Find(c => Contains(c.LicensePlate, text) || Contains(c.Make, text)
                               || Contains(c.Model, text) || Contains(c.OwnerName, text));
    }

    public List<Car> GetCarsSortedByYear(bool descending) => _cars.GetSorted(new CarYearComparer(descending));

    public List<RepairOrder> GetCarHistory(int carId)
    {
        _cars.GetById(carId);
        List<RepairOrder> history = _orders.Find(o => o.CarId == carId);
        history.Sort(new OrderDateComparer());
        return history;
    }

    // ------------------------------------------------------------ механіки

    public Mechanic AddMechanic(Mechanic mechanic)
    {
        _mechanics.Add(mechanic);
        MarkChanged();
        return mechanic;
    }

    public Mechanic GetMechanic(int id) => _mechanics[id];

    public void UpdateMechanic(int id, decimal hourlyRate, int maxActiveOrders)
    {
        Mechanic mechanic = _mechanics.GetById(id);
        var probe = new Mechanic(mechanic.FullName, mechanic.Specialization, hourlyRate, maxActiveOrders);
        mechanic.HourlyRate = probe.HourlyRate;
        mechanic.MaxActiveOrders = probe.MaxActiveOrders;
        MarkChanged();
    }

    public void RemoveMechanic(int id)
    {
        Mechanic mechanic = _mechanics.GetById(id);
        if (_orders.Exists(o => o.MechanicId == id))
            throw new EntityInUseException(mechanic.EntityName, id, "за ним закріплені замовлення");
        _mechanics.Remove(id);
        MarkChanged();
    }

    public int GetActiveOrderCount(int mechanicId) =>
        _orders.Find(o => o.MechanicId == mechanicId && o.IsActive).Count;

    public bool IsMechanicAvailable(Mechanic mechanic) =>
        GetActiveOrderCount(mechanic.Id) < mechanic.MaxActiveOrders;

    public List<Mechanic> GetMechanicsBySpecialization(Specialization specialization) =>
        _mechanics.Find(m => m.Specialization == specialization);

    public List<Mechanic> GetAvailableMechanics() => _mechanics.Find(IsMechanicAvailable);

    public List<Mechanic> GetMechanicsSortedByRate(bool descending) =>
        _mechanics.GetSorted(new MechanicRateComparer(descending));

    // ---------------------------------------------------------- запчастини

    public SparePart AddSparePart(SparePart part)
    {
        ArgumentNullException.ThrowIfNull(part);
        if (_parts.Exists(p => p.PartNumber == part.PartNumber))
            throw new DuplicateEntityException("Запчастина", part.PartNumber);
        _parts.Add(part);
        MarkChanged();
        return part;
    }

    public SparePart GetSparePart(int id) => _parts[id];

    public void RestockPart(int id, int quantity)
    {
        _parts.GetById(id).Restock(quantity);
        MarkChanged();
    }

    public void UpdatePartPrice(int id, decimal price)
    {
        _parts.GetById(id).Price = price;
        MarkChanged();
    }

    public void RemoveSparePart(int id)
    {
        SparePart part = _parts.GetById(id);
        if (_orders.Exists(o => o.IsActive && o.Parts.Any(p => p.PartId == id)))
            throw new EntityInUseException(part.EntityName, id, "вона використовується в незакритому замовленні");
        _parts.Remove(id);
        MarkChanged();
    }

    public List<SparePart> FindSpareParts(string query)
    {
        string text = (query ?? string.Empty).Trim();
        return _parts.Find(p => Contains(p.PartNumber, text) || Contains(p.Name, text));
    }

    public List<SparePart> GetLowStockParts() => _parts.Find(p => p.IsLowStock);

    public List<SparePart> GetPartsSortedByPrice(bool descending) =>
        _parts.GetSorted(new PartPriceComparer(descending));

    /// <summary>Вартість усіх запчастин на складі.</summary>
    public decimal GetStockValue() => _parts.GetAll().Sum(p => p.Price * p.QuantityInStock);

    // ---------------------------------------------------------- замовлення

    public RepairOrder CreateOrder(int carId, string problemDescription)
    {
        _cars.GetById(carId);
        var order = new RepairOrder(carId, problemDescription, _clock());
        _orders.Add(order);
        MarkChanged();
        return order;
    }

    public RepairOrder GetOrder(int id) => _orders[id];

    /// <summary>Призначити конкретного механіка. Якщо він зайнятий — MechanicBusyException.</summary>
    public void AssignMechanic(int orderId, int mechanicId)
    {
        RepairOrder order = _orders.GetById(orderId);
        Mechanic mechanic = _mechanics.GetById(mechanicId);
        if (order.MechanicId == mechanicId)
            return;
        int active = GetActiveOrderCount(mechanicId);
        if (active >= mechanic.MaxActiveOrders)
            throw new MechanicBusyException(mechanic.FullName, active, mechanic.MaxActiveOrders);
        order.AssignMechanic(mechanic);
        MarkChanged();
    }

    /// <summary>
    /// Автоматичне призначення: серед механіків потрібної спеціалізації обирається найменш завантажений,
    /// при рівності — з нижчою ставкою. Якщо вільних немає — MechanicBusyException.
    /// </summary>
    public Mechanic AutoAssignMechanic(int orderId, Specialization specialization)
    {
        RepairOrder order = _orders.GetById(orderId);
        Mechanic? best = _mechanics
            .Find(m => m.Specialization == specialization && IsMechanicAvailable(m))
            .OrderBy(GetLoadRatio)
            .ThenBy(m => m.HourlyRate)
            .FirstOrDefault();
        if (best is null)
            throw new MechanicBusyException(specialization.ToUkrainian());
        order.AssignMechanic(best);
        MarkChanged();
        return best;
    }

    /// <summary>Додати роботу за ставкою призначеного механіка.</summary>
    public RepairWork AddWork(int orderId, string description, decimal hours)
    {
        RepairOrder order = _orders.GetById(orderId);
        if (order.MechanicId is null)
            throw new InvalidOrderStateException(order.Id, order.Status, "додати роботу без механіка");
        Mechanic mechanic = _mechanics.GetById(order.MechanicId.Value);
        RepairWork work = order.AddWork(description, hours, mechanic.HourlyRate);
        MarkChanged();
        return work;
    }

    /// <summary>Списати запчастину зі складу в замовлення. Якщо не вистачає — PartOutOfStockException.</summary>
    public PartUsage AddPartToOrder(int orderId, int partId, int quantity)
    {
        RepairOrder order = _orders.GetById(orderId);
        SparePart part = _parts.GetById(partId);
        PartUsage usage = order.AddPart(part, quantity);
        MarkChanged();
        return usage;
    }

    public void CompleteOrder(int orderId)
    {
        _orders.GetById(orderId).Complete(_clock());
        MarkChanged();
    }

    /// <summary>Скасувати замовлення та повернути списані запчастини на склад.</summary>
    public void CancelOrder(int orderId)
    {
        RepairOrder order = _orders.GetById(orderId);
        foreach (PartUsage usage in order.Cancel(_clock()))
            _parts.FindById(usage.PartId)?.Return(usage.Quantity);
        MarkChanged();
    }

    public List<RepairOrder> GetOrdersByStatus(OrderStatus status) => _orders.Find(o => o.Status == status);

    public List<RepairOrder> GetActiveOrders() => _orders.Find(o => o.IsActive);

    public List<RepairOrder> GetOrdersByMechanic(int mechanicId) => _orders.Find(o => o.MechanicId == mechanicId);

    public List<RepairOrder> GetOrdersSortedByCost(bool descending) =>
        _orders.GetSorted(new OrderCostComparer(descending));

    // --------------------------------------------------------------- звіти

    public ShopStatistics GetStatistics()
    {
        List<RepairOrder> completed = GetOrdersByStatus(OrderStatus.Completed);
        return new ShopStatistics(
            GetActiveOrders().Count,
            completed.Count,
            GetOrdersByStatus(OrderStatus.Cancelled).Count,
            completed.Sum(o => o.LaborCost),
            completed.Sum(o => o.PartsCost));
    }

    /// <summary>Завантаженість і заробіток кожного механіка (за виконаними замовленнями).</summary>
    public List<MechanicReport> GetMechanicReports()
    {
        var reports = new List<MechanicReport>();
        foreach (Mechanic mechanic in _mechanics.GetAll())
        {
            List<RepairOrder> done = _orders.Find(o => o.MechanicId == mechanic.Id
                                                      && o.Status == OrderStatus.Completed);
            reports.Add(new MechanicReport(mechanic, GetActiveOrderCount(mechanic.Id), done.Count,
                                           done.Sum(o => o.TotalHours), done.Sum(o => o.LaborCost)));
        }

        reports.Sort((a, b) => b.LaborEarned.CompareTo(a.LaborEarned));
        return reports;
    }

    // ----------------------------------------------------------- допоміжне

    private double GetLoadRatio(Mechanic mechanic) =>
        (double)GetActiveOrderCount(mechanic.Id) / mechanic.MaxActiveOrders;

    private static bool Contains(string source, string text) =>
        source.Contains(text, StringComparison.CurrentCultureIgnoreCase);

    private void MarkChanged() => HasUnsavedChanges = true;

    private void Apply(ShopData data)
    {
        _cars.Load(data.Cars);
        _mechanics.Load(data.Mechanics);
        _parts.Load(data.SpareParts);
        _orders.Load(data.Orders);

        // Перевірка зв'язків між сутностями.
        foreach (RepairOrder order in _orders.GetAll())
        {
            if (_cars.FindById(order.CarId) is null)
                throw new ValidationException("ID автомобіля",
                    $"Замовлення №{order.Id} посилається на неіснуючий автомобіль {order.CarId}.");
            if (order.MechanicId is int mechanicId && _mechanics.FindById(mechanicId) is null)
                throw new ValidationException("ID механіка",
                    $"Замовлення №{order.Id} посилається на неіснуючого механіка {mechanicId}.");
        }
    }

    private void ClearAll()
    {
        _orders.Clear();
        _cars.Clear();
        _mechanics.Clear();
        _parts.Clear();
    }
}
