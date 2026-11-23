using AutoRepairShop.Core.Models;

namespace AutoRepairShop.Core.Comparers;

/// <summary>Сортування замовлень за сумарною вартістю, при рівності — за номером.</summary>
public sealed class OrderCostComparer : DirectionalComparer<RepairOrder>
{
    public OrderCostComparer(bool descending = true)
        : base(descending)
    {
    }

    protected override int CompareItems(RepairOrder x, RepairOrder y)
    {
        int byCost = x.CalculateCost().CompareTo(y.CalculateCost());
        return byCost != 0 ? byCost : x.Id.CompareTo(y.Id);
    }
}
