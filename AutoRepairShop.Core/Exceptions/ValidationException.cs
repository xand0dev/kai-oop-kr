namespace AutoRepairShop.Core.Exceptions;

/// <summary>Некоректне значення властивості сутності.</summary>
public class ValidationException : AutoRepairShopException
{
    public ValidationException(string propertyName, string message)
        : base(message)
    {
        PropertyName = propertyName;
    }

    public string PropertyName { get; }
}
