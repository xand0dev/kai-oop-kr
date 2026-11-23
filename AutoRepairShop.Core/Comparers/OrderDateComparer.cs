using AutoRepairShop.Core.Models;

namespace AutoRepairShop.Core.Comparers;

/// <summary>Сортування замовлень за датою створення.</summary>
public sealed class OrderDateComparer : DirectionalComparer<RepairOrder>
{
    public OrderDateComparer(bool descending = false)
        : base(descending)
    {
    }

    protected override int CompareItems(RepairOrder x, RepairOrder y)
    {
        int byDate = x.CreatedAt.CompareTo(y.CreatedAt);
        return byDate != 0 ? byDate : x.Id.CompareTo(y.Id);
    }
}
