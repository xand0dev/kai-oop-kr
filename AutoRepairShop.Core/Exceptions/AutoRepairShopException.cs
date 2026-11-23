namespace AutoRepairShop.Core.Exceptions;

/// <summary>Базовий клас усіх винятків бізнес-логіки автосервісу.</summary>
public class AutoRepairShopException : Exception
{
    public AutoRepairShopException(string message)
        : base(message)
    {
    }

    public AutoRepairShopException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
