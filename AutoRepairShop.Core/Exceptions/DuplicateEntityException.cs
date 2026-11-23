namespace AutoRepairShop.Core.Exceptions;

/// <summary>Спроба додати запис з унікальним ключем, який уже існує (держномер, артикул).</summary>
public class DuplicateEntityException : AutoRepairShopException
{
    public DuplicateEntityException(string entityName, string key)
        : base($"{entityName} з ключем «{key}» уже існує.")
    {
        EntityName = entityName;
        Key = key;
    }

    public string EntityName { get; }
    public string Key { get; }
}
