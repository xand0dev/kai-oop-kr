namespace AutoRepairShop.Core.Services;

/// <summary>Загальні показники автосервісу.</summary>
public sealed class ShopStatistics
{
    public ShopStatistics(int activeOrders, int completedOrders, int cancelledOrders,
                          decimal laborRevenue, decimal partsRevenue)
    {
        ActiveOrders = activeOrders;
        CompletedOrders = completedOrders;
        CancelledOrders = cancelledOrders;
        LaborRevenue = laborRevenue;
        PartsRevenue = partsRevenue;
    }

    public int ActiveOrders { get; }
    public int CompletedOrders { get; }
    public int CancelledOrders { get; }
    public decimal LaborRevenue { get; }
    public decimal PartsRevenue { get; }
    public decimal TotalRevenue => LaborRevenue + PartsRevenue;
    public decimal AverageCheck => CompletedOrders == 0 ? 0 : Math.Round(TotalRevenue / CompletedOrders, 2);
}
