using AutoRepairShop.Core.Exceptions;
using AutoRepairShop.Core.Models;
using AutoRepairShop.Core.Services;

namespace AutoRepairShop.Tests;

public class ServiceTests
{
    private readonly AutoRepairShopService _shop = TestShop.Create();

    [Fact]
    public void AddCar_DuplicatePlate_Throws()
    {
        _shop.AddCar(TestShop.Car());
        Assert.Throws<DuplicateEntityException>(() => _shop.AddCar(TestShop.Car()));
    }

    [Fact]
    public void Repository_AssignsSequentialIds()
    {
        Car first = _shop.AddCar(TestShop.Car("AA0001AA"));
        Car second = _shop.AddCar(TestShop.Car("AA0002AA"));
        Assert.Equal(1, first.Id);
        Assert.Equal(2, second.Id);
    }

    [Fact]
    public void FullRepair_CalculatesTotalCost()
    {
        Car car = _shop.AddCar(TestShop.Car());
        Mechanic mechanic = _shop.AddMechanic(TestShop.Mechanic(rate: 600m));
        SparePart part = _shop.AddSparePart(TestShop.Part(price: 300m, quantity: 5));

        RepairOrder order = _shop.CreateOrder(car.Id, "Планове ТО");
        _shop.AssignMechanic(order.Id, mechanic.Id);
        _shop.AddWork(order.Id, "Заміна оливи", 1.5m);
        _shop.AddPartToOrder(order.Id, part.Id, 2);
        _shop.AddPartToOrder(order.Id, part.Id, 1);
        _shop.CompleteOrder(order.Id);

        Assert.Equal(900m, order.LaborCost);
        Assert.Equal(900m, order.PartsCost);
        Assert.Equal(1800m, order.CalculateCost());
        Assert.Single(order.Parts); // повторне додавання збільшило кількість у тому ж рядку
        Assert.Equal(2, part.QuantityInStock);
        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.Equal(TestShop.Now, order.ClosedAt);
    }

    [Fact]
    public void AssignMechanic_WhenBusy_ThrowsMechanicBusy()
    {
        Mechanic mechanic = _shop.AddMechanic(TestShop.Mechanic(max: 1));
        Car car = _shop.AddCar(TestShop.Car());
        RepairOrder first = _shop.CreateOrder(car.Id, "Ремонт 1");
        RepairOrder second = _shop.CreateOrder(car.Id, "Ремонт 2");
        _shop.AssignMechanic(first.Id, mechanic.Id);

        var ex = Assert.Throws<MechanicBusyException>(() => _shop.AssignMechanic(second.Id, mechanic.Id));
        Assert.Equal(1, ex.ActiveOrders);
        Assert.Equal(OrderStatus.New, second.Status);
    }

    [Fact]
    public void Mechanic_BecomesFreeAfterCompletion()
    {
        Mechanic mechanic = _shop.AddMechanic(TestShop.Mechanic(max: 1));
        Car car = _shop.AddCar(TestShop.Car());
        RepairOrder first = _shop.CreateOrder(car.Id, "Ремонт 1");
        _shop.AssignMechanic(first.Id, mechanic.Id);
        _shop.AddWork(first.Id, "Діагностика", 1m);
        _shop.CompleteOrder(first.Id);

        RepairOrder second = _shop.CreateOrder(car.Id, "Ремонт 2");
        _shop.AssignMechanic(second.Id, mechanic.Id);
        Assert.Equal(mechanic.Id, second.MechanicId);
    }

    [Fact]
    public void AutoAssign_ChoosesLeastLoadedMechanic()
    {
        Car car = _shop.AddCar(TestShop.Car());
        Mechanic busy = _shop.AddMechanic(TestShop.Mechanic("Олег Ткаченко", max: 2));
        Mechanic free = _shop.AddMechanic(TestShop.Mechanic("Віктор Гнатюк", max: 2, rate: 900m));
        _shop.AssignMechanic(_shop.CreateOrder(car.Id, "Ремонт 1").Id, busy.Id);

        Mechanic chosen = _shop.AutoAssignMechanic(_shop.CreateOrder(car.Id, "Ремонт 2").Id, Specialization.Engine);
        Assert.Equal(free.Id, chosen.Id);
    }

    [Fact]
    public void AutoAssign_NoMechanicWithSpecialization_Throws()
    {
        Car car = _shop.AddCar(TestShop.Car());
        _shop.AddMechanic(TestShop.Mechanic(spec: Specialization.Engine));
        RepairOrder order = _shop.CreateOrder(car.Id, "Кузовний ремонт");
        Assert.Throws<MechanicBusyException>(() => _shop.AutoAssignMechanic(order.Id, Specialization.Bodywork));
    }

    [Fact]
    public void AddWork_WithoutMechanic_ThrowsInvalidState()
    {
        Car car = _shop.AddCar(TestShop.Car());
        RepairOrder order = _shop.CreateOrder(car.Id, "Ремонт");
        Assert.Throws<InvalidOrderStateException>(() => _shop.AddWork(order.Id, "Діагностика", 1m));
    }

    [Fact]
    public void AddPart_OutOfStock_DoesNotChangeOrder()
    {
        Car car = _shop.AddCar(TestShop.Car());
        SparePart part = _shop.AddSparePart(TestShop.Part(quantity: 1));
        RepairOrder order = _shop.CreateOrder(car.Id, "Ремонт");
        Assert.Throws<PartOutOfStockException>(() => _shop.AddPartToOrder(order.Id, part.Id, 2));
        Assert.Empty(order.Parts);
        Assert.Equal(1, part.QuantityInStock);
    }

    [Fact]
    public void CancelOrder_ReturnsPartsToStock()
    {
        Car car = _shop.AddCar(TestShop.Car());
        SparePart part = _shop.AddSparePart(TestShop.Part(quantity: 5));
        RepairOrder order = _shop.CreateOrder(car.Id, "Ремонт");
        _shop.AddPartToOrder(order.Id, part.Id, 3);
        _shop.CancelOrder(order.Id);
        Assert.Equal(5, part.QuantityInStock);
        Assert.Throws<InvalidOrderStateException>(() => _shop.CancelOrder(order.Id));
    }

    [Fact]
    public void CompleteOrder_WithoutWorks_Throws()
    {
        Car car = _shop.AddCar(TestShop.Car());
        Mechanic mechanic = _shop.AddMechanic(TestShop.Mechanic());
        RepairOrder order = _shop.CreateOrder(car.Id, "Ремонт");
        _shop.AssignMechanic(order.Id, mechanic.Id);
        Assert.Throws<InvalidOrderStateException>(() => _shop.CompleteOrder(order.Id));
    }

    [Fact]
    public void RemoveCar_WithOrders_ThrowsEntityInUse()
    {
        Car car = _shop.AddCar(TestShop.Car());
        _shop.CreateOrder(car.Id, "Ремонт");
        Assert.Throws<EntityInUseException>(() => _shop.RemoveCar(car.Id));
    }

    [Fact]
    public void GetOrder_Unknown_ThrowsNotFound() =>
        Assert.Throws<EntityNotFoundException>(() => _shop.GetOrder(42));

    [Fact]
    public void SortingAndFiltering_UseComparers()
    {
        _shop.AddSparePart(TestShop.Part("AAA-0001", 500m));
        _shop.AddSparePart(TestShop.Part("AAA-0002", 100m));
        _shop.AddSparePart(TestShop.Part("AAA-0003", 300m));
        Assert.Equal(new[] { 100m, 300m, 500m }, _shop.GetPartsSortedByPrice(false).Select(p => p.Price));
        Assert.Equal(new[] { 500m, 300m, 100m }, _shop.GetPartsSortedByPrice(true).Select(p => p.Price));
        Assert.Single(_shop.FindSpareParts("0002"));
    }

    [Fact]
    public void DemoData_HasExpectedStatistics()
    {
        _shop.LoadDemoData();
        ShopStatistics stats = _shop.GetStatistics();
        Assert.Equal(1, stats.CompletedOrders);
        Assert.Equal(2, stats.ActiveOrders);
        Assert.Equal(3020m, stats.TotalRevenue);
    }
}
