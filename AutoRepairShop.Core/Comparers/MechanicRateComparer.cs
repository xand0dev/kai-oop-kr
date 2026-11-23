using AutoRepairShop.Core.Models;

namespace AutoRepairShop.Core.Comparers;

/// <summary>Сортування механіків за погодинною ставкою.</summary>
public sealed class MechanicRateComparer : DirectionalComparer<Mechanic>
{
    public MechanicRateComparer(bool descending = false)
        : base(descending)
    {
    }

    protected override int CompareItems(Mechanic x, Mechanic y)
    {
        int byRate = x.HourlyRate.CompareTo(y.HourlyRate);
        return byRate != 0 ? byRate : x.CompareTo(y);
    }
}
