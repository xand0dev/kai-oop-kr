namespace AutoRepairShop.Core.Exceptions;

/// <summary>Механік (або всі механіки потрібної спеціалізації) зайнятий іншими замовленнями (виняток варіанта 22).</summary>
public class MechanicBusyException : AutoRepairShopException
{
    /// <summary>Конкретний механік уже має максимальну кількість активних замовлень.</summary>
    public MechanicBusyException(string mechanicName, int activeOrders, int maxActiveOrders)
        : base($"Механік {mechanicName} зайнятий: активних замовлень {activeOrders} з {maxActiveOrders} можливих.")
    {
        MechanicName = mechanicName;
        ActiveOrders = activeOrders;
        MaxActiveOrders = maxActiveOrders;
    }

    /// <summary>Немає жодного вільного механіка потрібної спеціалізації.</summary>
    public MechanicBusyException(string specialization)
        : base($"Немає вільного механіка зі спеціалізацією «{specialization}».")
    {
        MechanicName = string.Empty;
    }

    public string MechanicName { get; }
    public int ActiveOrders { get; }
    public int MaxActiveOrders { get; }
}
