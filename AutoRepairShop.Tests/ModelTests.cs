using AutoRepairShop.Core.Exceptions;
using AutoRepairShop.Core.Models;

namespace AutoRepairShop.Tests;

public class ModelTests
{
    [Theory]
    [InlineData("AA123BB")]
    [InlineData("ZZ1234ZZ")]
    [InlineData("")]
    public void Car_InvalidPlate_Throws(string plate) =>
        Assert.Throws<ValidationException>(() => TestShop.Car(plate));

    [Fact]
    public void Car_PlateIsNormalizedToUpper() => Assert.Equal("AA1234BB", TestShop.Car("aa1234bb").LicensePlate);

    [Fact]
    public void Car_MileageCannotDecrease()
    {
        Car car = TestShop.Car();
        Assert.Throws<ValidationException>(() => car.MileageKm = 50_000);
        Assert.Equal(100_000, car.MileageKm);
    }

    [Fact]
    public void Mechanic_RateOutOfRange_Throws() =>
        Assert.Throws<ValidationException>(() => TestShop.Mechanic(rate: 50m));

    [Fact]
    public void SparePart_TakeMoreThanStock_ThrowsPartOutOfStock()
    {
        SparePart part = TestShop.Part(quantity: 2);
        var ex = Assert.Throws<PartOutOfStockException>(() => part.Take(3));
        Assert.Equal(3, ex.Requested);
        Assert.Equal(2, ex.Available);
        Assert.Equal(2, part.QuantityInStock);
    }

    [Fact]
    public void RepairWork_CostIsHoursTimesRate() =>
        Assert.Equal(975m, new RepairWork("Заміна ременя", 1.5m, 650m).CalculateCost());

    [Fact]
    public void OrderLines_ArePolymorphic()
    {
        OrderLine[] lines = { new RepairWork("Діагностика", 1m, 500m), new PartUsage(1, "FLT-0101", "Фільтр", 2, 300m) };
        Assert.Equal(1100m, lines.Sum(l => l.CalculateCost()));
    }
}
