using AutoRepairShop.Core.Interfaces;
using AutoRepairShop.Core.Models;
using AutoRepairShop.Core.Services;
using AutoRepairShop.Core.Storage;

namespace AutoRepairShop.Tests;

/// <summary>Сховище в пам'яті замість файла.</summary>
internal sealed class InMemoryStorage : IDataStorage
{
    public ShopData? Saved { get; private set; }
    public string Location => "memory";
    public bool Exists => Saved is not null;
    public ShopData Load() => Saved ?? new ShopData();
    public void Save(ShopData data) => Saved = data;
}

internal static class TestShop
{
    public static readonly DateTime Now = new(2026, 11, 19, 10, 0, 0);

    public static AutoRepairShopService Create(IDataStorage? storage = null) =>
        new(storage ?? new InMemoryStorage(), () => Now);

    public static Car Car(string plate = "AA1234BB") =>
        new(plate, "Volkswagen", "Golf", 2015, "Іван Петренко", "+380671234567", 100_000);

    public static Mechanic Mechanic(string name = "Сергій Мельник", Specialization spec = Specialization.Engine,
                                    decimal rate = 600m, int max = 1) => new(name, spec, rate, max);

    public static SparePart Part(string number = "FLT-0101", decimal price = 300m, int quantity = 5) =>
        new(number, "Масляний фільтр", price, quantity, 1);
}
