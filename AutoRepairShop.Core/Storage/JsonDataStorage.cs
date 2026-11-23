using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoRepairShop.Core.Exceptions;
using AutoRepairShop.Core.Interfaces;

namespace AutoRepairShop.Core.Storage;

/// <summary>Збереження даних через серіалізацію System.Text.Json.</summary>
public class JsonDataStorage : IDataStorage
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // кирилиця у файлі без \uXXXX
        Converters = { new JsonStringEnumConverter() },         // статуси як текст, а не числа
    };

    private readonly string _filePath;

    public JsonDataStorage(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Шлях до файла не може бути порожнім.", nameof(filePath));
        _filePath = Path.GetFullPath(filePath);
    }

    public string Location => _filePath;

    public bool Exists => File.Exists(_filePath);

    public ShopData Load()
    {
        if (!Exists)
            return new ShopData();
        try
        {
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<ShopData>(json, Options) ?? new ShopData();
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException
                                       or AutoRepairShopException or NotSupportedException)
        {
            throw new DataStorageException("Не вдалося прочитати файл даних", _filePath, ex);
        }
    }

    public void Save(ShopData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        try
        {
            string? directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            // Спочатку пишемо тимчасовий файл, потім підміняємо: старі дані не зіпсуються при збої.
            string tempPath = _filePath + ".tmp";
            File.WriteAllText(tempPath, JsonSerializer.Serialize(data, Options));
            File.Move(tempPath, _filePath, overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            throw new DataStorageException("Не вдалося зберегти файл даних", _filePath, ex);
        }
    }
}
