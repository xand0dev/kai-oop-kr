using AutoRepairShop.Core.Models;

namespace AutoRepairShop.Core.Storage;

/// <summary>Знімок усіх даних автосервісу, який серіалізується в один JSON-файл.</summary>
public class ShopData
{
    public int FormatVersion { get; set; } = 1;
    public DateTime SavedAt { get; set; }
    public List<Car> Cars { get; set; } = new();
    public List<Mechanic> Mechanics { get; set; } = new();
    public List<SparePart> SpareParts { get; set; } = new();
    public List<RepairOrder> Orders { get; set; } = new();
}
