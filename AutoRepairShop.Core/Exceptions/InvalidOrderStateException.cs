using AutoRepairShop.Core.Models;

namespace AutoRepairShop.Core.Exceptions;

/// <summary>Дію не можна виконати для замовлення в поточному статусі.</summary>
public class InvalidOrderStateException : AutoRepairShopException
{
    public InvalidOrderStateException(int orderId, OrderStatus status, string action)
        : base($"Замовлення №{orderId} має статус «{status.ToUkrainian()}», тому не можна {action}.")
    {
        OrderId = orderId;
        Status = status;
    }

    public int OrderId { get; }
    public OrderStatus Status { get; }
}
