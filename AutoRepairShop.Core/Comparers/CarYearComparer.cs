using AutoRepairShop.Core.Models;

namespace AutoRepairShop.Core.Comparers;

/// <summary>Сортування автомобілів за роком випуску, при рівності — за держномером.</summary>
public sealed class CarYearComparer : DirectionalComparer<Car>
{
    public CarYearComparer(bool descending = false)
        : base(descending)
    {
    }

    protected override int CompareItems(Car x, Car y)
    {
        int byYear = x.Year.CompareTo(y.Year);
        return byYear != 0 ? byYear : x.CompareTo(y);
    }
}
