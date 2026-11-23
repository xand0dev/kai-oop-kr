using AutoRepairShop.Core.Models;

namespace AutoRepairShop.Core.Comparers;

/// <summary>Сортування запчастин за ціною, при рівності — за назвою.</summary>
public sealed class PartPriceComparer : DirectionalComparer<SparePart>
{
    public PartPriceComparer(bool descending = false)
        : base(descending)
    {
    }

    protected override int CompareItems(SparePart x, SparePart y)
    {
        int byPrice = x.Price.CompareTo(y.Price);
        return byPrice != 0 ? byPrice : x.CompareTo(y);
    }
}
