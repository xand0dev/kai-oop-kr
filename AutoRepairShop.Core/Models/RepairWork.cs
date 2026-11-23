using System.Text.Json.Serialization;
using AutoRepairShop.Core.Validation;

namespace AutoRepairShop.Core.Models;

/// <summary>Виконана ремонтна робота: опис, нормогодини та ставка механіка на момент виконання.</summary>
public sealed class RepairWork : OrderLine
{
    [JsonConstructor]
    public RepairWork(string description, decimal hours, decimal hourlyRate)
    {
        Description = Guard.Text(description, "Опис роботи", 3, 100);
        Hours = Guard.Range(hours, "Нормогодини", ValidationRules.MinWorkHours, ValidationRules.MaxWorkHours);
        HourlyRate = Guard.Range(hourlyRate, "Ставка за годину",
                                 ValidationRules.MinHourlyRate, ValidationRules.MaxHourlyRate);
    }

    public string Description { get; }
    public decimal Hours { get; }

    /// <summary>Ставка фіксується в рядку, щоб зміна ставки механіка не змінила старі чеки.</summary>
    public decimal HourlyRate { get; }

    public override decimal CalculateCost() => Math.Round(Hours * HourlyRate, 2);

    public override string Describe() => $"Робота: {Description}, {Hours:0.##} год × {HourlyRate:0.00} грн";
}
