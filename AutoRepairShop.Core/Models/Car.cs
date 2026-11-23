using System.Text.Json.Serialization;
using AutoRepairShop.Core.Validation;

namespace AutoRepairShop.Core.Models;

/// <summary>Автомобіль клієнта, що обслуговується на СТО.</summary>
public class Car : EntityBase, IComparable<Car>
{
    private string _licensePlate = string.Empty;
    private string _make = string.Empty;
    private string _model = string.Empty;
    private int _year;
    private string _ownerName = string.Empty;
    private string _ownerPhone = string.Empty;
    private int _mileageKm;

    /// <summary>Конструктор для десеріалізації JSON; поля перевіряються методом Validate після завантаження.</summary>
    [JsonConstructor]
    private Car()
    {
    }

    public Car(string licensePlate, string make, string model, int year,
               string ownerName, string ownerPhone, int mileageKm)
    {
        LicensePlate = licensePlate;
        Make = make;
        Model = model;
        Year = year;
        OwnerName = ownerName;
        OwnerPhone = ownerPhone;
        MileageKm = mileageKm;
    }

    [JsonIgnore]
    public override string EntityName => "Автомобіль";

    /// <summary>Держномер — унікальний ключ автомобіля, після реєстрації не змінюється.</summary>
    [JsonInclude]
    public string LicensePlate
    {
        get => _licensePlate;
        private set => _licensePlate = Guard.Pattern(value, "Держномер", ValidationRules.LicensePlate,
                                                     ValidationRules.LicensePlateHint);
    }

    public string Make
    {
        get => _make;
        set => _make = Guard.Text(value, "Марка", 2, 30);
    }

    public string Model
    {
        get => _model;
        set => _model = Guard.Text(value, "Модель", 1, 30);
    }

    public int Year
    {
        get => _year;
        set => _year = Guard.Range(value, "Рік випуску", ValidationRules.MinCarYear, ValidationRules.MaxCarYear);
    }

    public string OwnerName
    {
        get => _ownerName;
        set => _ownerName = Guard.Name(value, "Власник", 60);
    }

    public string OwnerPhone
    {
        get => _ownerPhone;
        set => _ownerPhone = Guard.Pattern(value, "Телефон", ValidationRules.Phone, ValidationRules.PhoneHint);
    }

    /// <summary>Пробіг у кілометрах; не може зменшуватися.</summary>
    public int MileageKm
    {
        get => _mileageKm;
        set
        {
            int mileage = Guard.Range(value, "Пробіг", 0, ValidationRules.MaxMileageKm);
            if (mileage < _mileageKm)
                throw new Exceptions.ValidationException("Пробіг",
                    $"Пробіг не може зменшитися: було {_mileageKm} км, введено {mileage} км.");
            _mileageKm = mileage;
        }
    }

    public override string GetSummary() => $"{LicensePlate} {Make} {Model} ({Year})";

    public override void Validate()
    {
        LicensePlate = _licensePlate;
        Make = _make;
        Model = _model;
        Year = _year;
        OwnerName = _ownerName;
        OwnerPhone = _ownerPhone;
        Guard.Range(_mileageKm, "Пробіг", 0, ValidationRules.MaxMileageKm);
    }

    /// <summary>Природний порядок — за держномером.</summary>
    public int CompareTo(Car? other) =>
        other is null ? 1 : string.Compare(LicensePlate, other.LicensePlate, StringComparison.Ordinal);
}
