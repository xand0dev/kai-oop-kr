using System.Text.RegularExpressions;

namespace AutoRepairShop.Core.Validation;

/// <summary>
/// Правила коректності даних. Спільні для бізнес-логіки та інтерфейсу:
/// UI перевіряє введення за ними ще до створення об'єктів.
/// </summary>
public static class ValidationRules
{
    /// <summary>Український держномер латиницею: дві літери, чотири цифри, дві літери (AA1234BB).</summary>
    public static readonly Regex LicensePlate = new(@"^[ABCEHIKMOPTX]{2}\d{4}[ABCEHIKMOPTX]{2}$", RegexOptions.Compiled);

    /// <summary>Мобільний номер України у форматі +380XXXXXXXXX.</summary>
    public static readonly Regex Phone = new(@"^\+380\d{9}$", RegexOptions.Compiled);

    /// <summary>Артикул запчастини: три великі латинські літери, дефіс, чотири цифри (BRK-0001).</summary>
    public static readonly Regex PartNumber = new(@"^[A-Z]{3}-\d{4}$", RegexOptions.Compiled);

    /// <summary>Ім'я людини: літери (укр./лат.), пробіл, апостроф, дефіс.</summary>
    public static readonly Regex PersonName = new(@"^[A-Za-zА-ЩЬЮЯҐЄІЇа-щьюяґєії' \-]+$", RegexOptions.Compiled);

    public const string LicensePlateHint = "AA1234BB (латиниця: A B C E H I K M O P T X)";
    public const string PhoneHint = "+380XXXXXXXXX";
    public const string PartNumberHint = "ABC-1234";

    public const int MinCarYear = 1950;
    public const int MaxMileageKm = 2_000_000;
    public const decimal MinHourlyRate = 100m;
    public const decimal MaxHourlyRate = 5_000m;
    public const int MaxActiveOrdersLimit = 5;
    public const decimal MaxPartPrice = 1_000_000m;
    public const int MaxStockQuantity = 100_000;
    public const decimal MinWorkHours = 0.1m;
    public const decimal MaxWorkHours = 100m;

    /// <summary>Найпізніший допустимий рік випуску — наступний календарний рік.</summary>
    public static int MaxCarYear => DateTime.Now.Year + 1;
}
