using AutoRepairShop.Core.Models;
using AutoRepairShop.Core.Services;

namespace AutoRepairShop.UI.ConsoleTools;

/// <summary>Перетворення сутностей на рядки таблиць для виведення.</summary>
public static class Rows
{
    public static readonly string[] CarHeaders = { "ID", "Держномер", "Авто", "Рік", "Власник", "Телефон", "Пробіг, км" };
    public static readonly string[] MechanicHeaders = { "ID", "ПІБ", "Спеціалізація", "Ставка, грн/год", "Завантаження" };
    public static readonly string[] PartHeaders = { "ID", "Артикул", "Назва", "Ціна, грн", "На складі", "Мін." };
    public static readonly string[] OrderHeaders = { "№", "Створено", "Авто", "Механік", "Статус", "Роботи", "Запч.", "Сума, грн" };

    public static IReadOnlyList<string> Car(Car car) => new[]
    {
        car.Id.ToString(), car.LicensePlate, $"{car.Make} {car.Model}", car.Year.ToString(),
        car.OwnerName, car.OwnerPhone, car.MileageKm.ToString("N0"),
    };

    public static IReadOnlyList<string> Mechanic(Mechanic mechanic, AutoRepairShopService shop) => new[]
    {
        mechanic.Id.ToString(), mechanic.FullName, mechanic.Specialization.ToUkrainian(),
        mechanic.HourlyRate.ToString("0.00"),
        $"{shop.GetActiveOrderCount(mechanic.Id)}/{mechanic.MaxActiveOrders}"
            + (shop.IsMechanicAvailable(mechanic) ? string.Empty : " зайнятий"),
    };

    public static IReadOnlyList<string> Part(SparePart part) => new[]
    {
        part.Id.ToString(), part.PartNumber, part.Name, part.Price.ToString("0.00"),
        part.QuantityInStock + (part.IsLowStock ? " ⚠" : string.Empty), part.MinStockLevel.ToString(),
    };

    public static IReadOnlyList<string> Order(RepairOrder order, AutoRepairShopService shop) => new[]
    {
        order.Id.ToString(), order.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
        shop.Cars.FirstOrDefault(c => c.Id == order.CarId)?.LicensePlate ?? "?",
        order.MechanicId is int id ? shop.Mechanics.FirstOrDefault(m => m.Id == id)?.FullName ?? "?" : "—",
        order.Status.ToUkrainian(), order.Works.Count.ToString(), order.Parts.Count.ToString(),
        order.CalculateCost().ToString("0.00"),
    };
}
