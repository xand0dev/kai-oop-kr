namespace AutoRepairShop.Core.Exceptions;

/// <summary>Запис не можна видалити, бо на нього посилаються замовлення.</summary>
public class EntityInUseException : AutoRepairShopException
{
    public EntityInUseException(string entityName, int id, string reason)
        : base($"{entityName} з ID {id} не можна видалити: {reason}.")
    {
        EntityName = entityName;
        Id = id;
    }

    public string EntityName { get; }
    public int Id { get; }
}
