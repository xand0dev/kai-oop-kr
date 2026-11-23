using System.Text.Json.Serialization;
using AutoRepairShop.Core.Validation;

namespace AutoRepairShop.Core.Models;

/// <summary>Запчастина, використана в замовленні: кількість і ціна на момент списання.</summary>
public sealed class PartUsage : OrderLine
{
    [JsonConstructor]
    public PartUsage(int partId, string partNumber, string partName, int quantity, decimal unitPrice)
    {
        PartId = Guard.Range(partId, "ID запчастини", 1, int.MaxValue);
        PartNumber = Guard.Pattern(partNumber, "Артикул", ValidationRules.PartNumber, ValidationRules.PartNumberHint);
        PartName = Guard.Text(partName, "Назва запчастини", 2, 60);
        Quantity = Guard.Range(quantity, "Кількість", 1, ValidationRules.MaxStockQuantity);
        UnitPrice = Guard.Range(unitPrice, "Ціна", 0.01m, ValidationRules.MaxPartPrice);
    }

    public int PartId { get; }
    public string PartNumber { get; }
    public string PartName { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }

    public override decimal CalculateCost() => Math.Round(Quantity * UnitPrice, 2);

    public override string Describe() => $"Запчастина: {PartNumber} {PartName}, {Quantity} шт. × {UnitPrice:0.00} грн";

    /// <summary>Повторне додавання тієї ж запчастини збільшує кількість у рядку.</summary>
    internal PartUsage WithAddedQuantity(int quantity) =>
        new(PartId, PartNumber, PartName, Quantity + quantity, UnitPrice);
}
