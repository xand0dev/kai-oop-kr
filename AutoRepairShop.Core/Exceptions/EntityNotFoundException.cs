namespace AutoRepairShop.Core.Exceptions;

/// <summary>Запис із заданим ідентифікатором не знайдено.</summary>
public class EntityNotFoundException : AutoRepairShopException
{
    public EntityNotFoundException(string entityName, int id)
        : base($"{entityName} з ID {id} не знайдено.")
    {
        EntityName = entityName;
        Id = id;
    }

    public string EntityName { get; }
    public int Id { get; }
}
