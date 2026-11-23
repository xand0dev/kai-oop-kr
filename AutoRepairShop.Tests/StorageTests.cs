using AutoRepairShop.Core.Exceptions;
using AutoRepairShop.Core.Models;
using AutoRepairShop.Core.Services;
using AutoRepairShop.Core.Storage;

namespace AutoRepairShop.Tests;

public sealed class StorageTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"shop-{Guid.NewGuid():N}.json");

    public void Dispose()
    {
        if (File.Exists(_path))
            File.Delete(_path);
    }

    [Fact]
    public void SaveAndLoad_RestoresAllData()
    {
        AutoRepairShopService original = TestShop.Create(new JsonDataStorage(_path));
        original.LoadDemoData();
        original.Save();

        AutoRepairShopService restored = TestShop.Create(new JsonDataStorage(_path));
        Assert.True(restored.Load());
        Assert.Equal(original.Cars.Count, restored.Cars.Count);
        Assert.Equal(original.Orders.Count, restored.Orders.Count);
        Assert.Equal(original.Orders.Sum(o => o.CalculateCost()), restored.Orders.Sum(o => o.CalculateCost()));
        Assert.Equal(original.SpareParts.Sum(p => p.QuantityInStock), restored.SpareParts.Sum(p => p.QuantityInStock));
        Assert.False(restored.HasUnsavedChanges);

        // Нові записи продовжують нумерацію після завантаження.
        Car car = restored.AddCar(TestShop.Car("XX9999XX"));
        Assert.Equal(5, car.Id);
    }

    [Fact]
    public void Load_MissingFile_ReturnsFalse() =>
        Assert.False(TestShop.Create(new JsonDataStorage(_path)).Load());

    [Fact]
    public void Load_BrokenJson_ThrowsDataStorageException()
    {
        File.WriteAllText(_path, "{ not json");
        Assert.Throws<DataStorageException>(() => TestShop.Create(new JsonDataStorage(_path)).Load());
    }

    [Fact]
    public void Load_InvalidValue_ThrowsDataStorageException()
    {
        AutoRepairShopService shop = TestShop.Create(new JsonDataStorage(_path));
        shop.LoadDemoData();
        shop.Save();
        File.WriteAllText(_path, File.ReadAllText(_path).Replace("+380671234567", "12345"));
        Assert.Throws<DataStorageException>(() => TestShop.Create(new JsonDataStorage(_path)).Load());
    }
}
