using System.Text.Json.Serialization;
using AutoRepairShop.Core.Validation;

namespace AutoRepairShop.Core.Models;

/// <summary>Механік СТО: спеціалізація, погодинна ставка та максимум одночасних замовлень.</summary>
public class Mechanic : EntityBase, IComparable<Mechanic>
{
    private string _fullName = string.Empty;
    private Specialization _specialization;
    private decimal _hourlyRate;
    private int _maxActiveOrders;

    [JsonConstructor]
    private Mechanic()
    {
    }

    public Mechanic(string fullName, Specialization specialization, decimal hourlyRate, int maxActiveOrders)
    {
        FullName = fullName;
        Specialization = specialization;
        HourlyRate = hourlyRate;
        MaxActiveOrders = maxActiveOrders;
    }

    [JsonIgnore]
    public override string EntityName => "Механік";

    public string FullName
    {
        get => _fullName;
        set => _fullName = Guard.Name(value, "ПІБ механіка", 60);
    }

    public Specialization Specialization
    {
        get => _specialization;
        set => _specialization = Guard.Defined(value, "Спеціалізація");
    }

    /// <summary>Вартість однієї нормогодини роботи, грн.</summary>
    public decimal HourlyRate
    {
        get => _hourlyRate;
        set => _hourlyRate = Guard.Range(value, "Ставка за годину",
                                         ValidationRules.MinHourlyRate, ValidationRules.MaxHourlyRate);
    }

    /// <summary>Скільки замовлень механік може вести одночасно; більше — MechanicBusyException.</summary>
    public int MaxActiveOrders
    {
        get => _maxActiveOrders;
        set => _maxActiveOrders = Guard.Range(value, "Максимум замовлень", 1, ValidationRules.MaxActiveOrdersLimit);
    }

    public override string GetSummary() =>
        $"{FullName} — {Specialization.ToUkrainian()}, {HourlyRate:0.00} грн/год";

    public override void Validate()
    {
        FullName = _fullName;
        Specialization = _specialization;
        HourlyRate = _hourlyRate;
        MaxActiveOrders = _maxActiveOrders;
    }

    public int CompareTo(Mechanic? other) =>
        other is null ? 1 : string.Compare(FullName, other.FullName, StringComparison.CurrentCulture);
}
