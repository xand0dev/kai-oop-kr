using AutoRepairShop.Core.Models;

namespace AutoRepairShop.Core.Services;

/// <summary>Підсумок роботи механіка: завантаженість і заробіток за виконаними замовленнями.</summary>
public sealed class MechanicReport
{
    public MechanicReport(Mechanic mechanic, int activeOrders, int completedOrders, decimal hoursWorked, decimal laborEarned)
    {
        Mechanic = mechanic;
        ActiveOrders = activeOrders;
        CompletedOrders = completedOrders;
        HoursWorked = hoursWorked;
        LaborEarned = laborEarned;
    }

    public Mechanic Mechanic { get; }
    public int ActiveOrders { get; }
    public int CompletedOrders { get; }
    public decimal HoursWorked { get; }
    public decimal LaborEarned { get; }
    public bool IsBusy => ActiveOrders >= Mechanic.MaxActiveOrders;
}
