namespace AutoRepairShop.Core.Models;

/// <summary>Етапи життєвого циклу замовлення на ремонт.</summary>
public enum OrderStatus
{
    /// <summary>Створене, механіка ще не призначено.</summary>
    New,

    /// <summary>Механік призначений, триває ремонт.</summary>
    InProgress,

    /// <summary>Ремонт завершено, вартість зафіксовано.</summary>
    Completed,

    /// <summary>Скасоване, запчастини повернуто на склад.</summary>
    Cancelled,
}
