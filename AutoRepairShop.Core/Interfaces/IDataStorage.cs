using AutoRepairShop.Core.Storage;

namespace AutoRepairShop.Core.Interfaces;

/// <summary>Сховище, що зберігає та відновлює всі дані автосервісу.</summary>
public interface IDataStorage
{
    /// <summary>Шлях або опис місця збереження.</summary>
    string Location { get; }

    bool Exists { get; }

    ShopData Load();
    void Save(ShopData data);
}
