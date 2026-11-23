namespace AutoRepairShop.Core.Exceptions;

/// <summary>Помилка читання або запису файла даних.</summary>
public class DataStorageException : AutoRepairShopException
{
    public DataStorageException(string message, string location, Exception innerException)
        : base($"{message} ({location}): {innerException.Message}", innerException)
    {
        Location = location;
    }

    public string Location { get; }
}
