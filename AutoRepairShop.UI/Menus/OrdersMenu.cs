using AutoRepairShop.Core.Models;
using AutoRepairShop.Core.Services;
using AutoRepairShop.Core.Validation;
using AutoRepairShop.UI.ConsoleTools;

namespace AutoRepairShop.UI.Menus;

/// <summary>Замовлення на ремонт: створення, призначення механіка, роботи, запчастини, закриття.</summary>
public sealed class OrdersMenu : MenuBase
{
    public OrdersMenu(AutoRepairShopService shop)
        : base(shop)
    {
    }

    protected override string Title => "Замовлення на ремонт";

    protected override IReadOnlyList<MenuItem> CreateItems() => new[]
    {
        new MenuItem("1", "Усі замовлення", () => Print(Shop.Orders)),
        new MenuItem("2", "Створити замовлення", Create),
        new MenuItem("3", "Призначити механіка вручну", AssignManually),
        new MenuItem("4", "Призначити механіка автоматично", AssignAutomatically),
        new MenuItem("5", "Додати виконану роботу", AddWork),
        new MenuItem("6", "Додати запчастину зі складу", AddPart),
        new MenuItem("7", "Розрахунок вартості (чек)", ShowReceipt),
        new MenuItem("8", "Завершити ремонт", Complete),
        new MenuItem("9", "Скасувати замовлення", Cancel),
        new MenuItem("10", "Фільтр за статусом", FilterByStatus),
        new MenuItem("11", "Сортувати за вартістю", () => Print(Shop.GetOrdersSortedByCost(ConsoleInput.Confirm("Спочатку найдорожчі?")))),
    };

    private void Create()
    {
        Car car = Shop.GetCar(ConsoleInput.ReadId("ID автомобіля"));
        ConsoleView.Info(car.ToString());
        string problem = ConsoleInput.ReadText("Опис несправності", 3, 200);
        RepairOrder order = Shop.CreateOrder(car.Id, problem);
        ConsoleView.Success($"Створено замовлення №{order.Id}. Наступний крок — призначити механіка.");
    }

    private void AssignManually()
    {
        RepairOrder order = Shop.GetOrder(ConsoleInput.ReadId("№ замовлення"));
        ConsoleView.Table(Rows.MechanicHeaders, Shop.Mechanics.Select(m => Rows.Mechanic(m, Shop)));
        Mechanic mechanic = Shop.GetMechanic(ConsoleInput.ReadId("ID механіка"));
        Shop.AssignMechanic(order.Id, mechanic.Id);
        ConsoleView.Success($"Замовлення №{order.Id} передано механіку {mechanic.FullName}.");
    }

    private void AssignAutomatically()
    {
        RepairOrder order = Shop.GetOrder(ConsoleInput.ReadId("№ замовлення"));
        Specialization specialization = ConsoleInput.ReadEnum<Specialization>("Потрібна спеціалізація", s => s.ToUkrainian());
        Mechanic mechanic = Shop.AutoAssignMechanic(order.Id, specialization);
        ConsoleView.Success($"Обрано найменш завантаженого механіка: {mechanic.GetSummary()}.");
    }

    private void AddWork()
    {
        RepairOrder order = Shop.GetOrder(ConsoleInput.ReadId("№ замовлення"));
        string description = ConsoleInput.ReadText("Опис роботи", 3, 100);
        decimal hours = ConsoleInput.ReadDecimal("Нормогодини", ValidationRules.MinWorkHours, ValidationRules.MaxWorkHours);
        RepairWork work = Shop.AddWork(order.Id, description, hours);
        ConsoleView.Success($"Додано: {work}");
    }

    private void AddPart()
    {
        RepairOrder order = Shop.GetOrder(ConsoleInput.ReadId("№ замовлення"));
        SparePart part = Shop.GetSparePart(ConsoleInput.ReadId("ID запчастини"));
        ConsoleView.Info(part.GetSummary());
        int quantity = ConsoleInput.ReadInt("Кількість", 1, 1_000);
        PartUsage usage = Shop.AddPartToOrder(order.Id, part.Id, quantity);
        ConsoleView.Success($"Списано зі складу: {usage}. Залишок: {part.QuantityInStock} шт.");
    }

    private void ShowReceipt()
    {
        RepairOrder order = Shop.GetOrder(ConsoleInput.ReadId("№ замовлення"));
        Car car = Shop.GetCar(order.CarId);
        ConsoleView.Title($"Чек замовлення №{order.Id}");
        Console.WriteLine($"Авто:      {car.GetSummary()}, власник {car.OwnerName}");
        Console.WriteLine($"Проблема:  {order.ProblemDescription}");
        Console.WriteLine($"Механік:   {(order.MechanicId is int id ? Shop.GetMechanic(id).GetSummary() : "не призначено")}");
        Console.WriteLine($"Статус:    {order.Status.ToUkrainian()}");
        Console.WriteLine(new string('-', 60));
        foreach (OrderLine line in order.Lines) // поліморфізм: RepairWork і PartUsage рахують вартість по-своєму
            Console.WriteLine($"  {line}");
        Console.WriteLine(new string('-', 60));
        Console.WriteLine($"Роботи ({order.TotalHours:0.##} год): {order.LaborCost,12:0.00} грн");
        Console.WriteLine($"Запчастини:             {order.PartsCost,12:0.00} грн");
        ConsoleView.Success($"РАЗОМ ДО СПЛАТИ:        {order.CalculateCost(),12:0.00} грн");
    }

    private void Complete()
    {
        RepairOrder order = Shop.GetOrder(ConsoleInput.ReadId("№ замовлення"));
        Shop.CompleteOrder(order.Id);
        ConsoleView.Success($"Замовлення №{order.Id} виконано. До сплати {order.CalculateCost():0.00} грн.");
    }

    private void Cancel()
    {
        RepairOrder order = Shop.GetOrder(ConsoleInput.ReadId("№ замовлення"));
        if (!ConsoleInput.Confirm($"Скасувати замовлення №{order.Id}? Запчастини повернуться на склад"))
            return;
        Shop.CancelOrder(order.Id);
        ConsoleView.Success("Замовлення скасовано, запчастини повернуто на склад.");
    }

    private void FilterByStatus()
    {
        OrderStatus status = ConsoleInput.ReadEnum<OrderStatus>("Статус", s => s.ToUkrainian());
        Print(Shop.GetOrdersByStatus(status));
    }

    private void Print(IEnumerable<RepairOrder> orders) =>
        ConsoleView.Table(Rows.OrderHeaders, orders.Select(o => Rows.Order(o, Shop)));
}
