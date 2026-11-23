using System.Text.Json.Serialization;
using AutoRepairShop.Core.Exceptions;
using AutoRepairShop.Core.Validation;

namespace AutoRepairShop.Core.Models;

/// <summary>Запчастина на складі СТО. Кількість змінюється лише методами Take / Restock / Return.</summary>
public class SparePart : EntityBase, IComparable<SparePart>
{
    private string _partNumber = string.Empty;
    private string _name = string.Empty;
    private decimal _price;
    private int _quantityInStock;
    private int _minStockLevel;

    [JsonConstructor]
    private SparePart()
    {
    }

    public SparePart(string partNumber, string name, decimal price, int quantityInStock, int minStockLevel)
    {
        PartNumber = partNumber;
        Name = name;
        Price = price;
        QuantityInStock = quantityInStock;
        MinStockLevel = minStockLevel;
    }

    [JsonIgnore]
    public override string EntityName => "Запчастина";

    /// <summary>Артикул — унікальний ключ запчастини.</summary>
    [JsonInclude]
    public string PartNumber
    {
        get => _partNumber;
        private set => _partNumber = Guard.Pattern(value, "Артикул", ValidationRules.PartNumber,
                                                   ValidationRules.PartNumberHint);
    }

    public string Name
    {
        get => _name;
        set => _name = Guard.Text(value, "Назва запчастини", 2, 60);
    }

    /// <summary>Ціна за одиницю, грн.</summary>
    public decimal Price
    {
        get => _price;
        set => _price = Guard.Range(value, "Ціна", 0.01m, ValidationRules.MaxPartPrice);
    }

    [JsonInclude]
    public int QuantityInStock
    {
        get => _quantityInStock;
        private set => _quantityInStock = Guard.Range(value, "Кількість на складі", 0,
                                                      ValidationRules.MaxStockQuantity);
    }

    /// <summary>Мінімальний залишок; нижче нього запчастина потрапляє у звіт «закінчується».</summary>
    public int MinStockLevel
    {
        get => _minStockLevel;
        set => _minStockLevel = Guard.Range(value, "Мінімальний залишок", 0, 1_000);
    }

    [JsonIgnore]
    public bool IsLowStock => QuantityInStock <= MinStockLevel;

    /// <summary>Списати зі складу для ремонту. Якщо не вистачає — PartOutOfStockException.</summary>
    public void Take(int quantity)
    {
        Guard.Range(quantity, "Кількість", 1, ValidationRules.MaxStockQuantity);
        if (quantity > QuantityInStock)
            throw new PartOutOfStockException(PartNumber, Name, quantity, QuantityInStock);
        QuantityInStock -= quantity;
    }

    /// <summary>Поповнення складу від постачальника.</summary>
    public void Restock(int quantity)
    {
        Guard.Range(quantity, "Кількість", 1, ValidationRules.MaxStockQuantity);
        QuantityInStock += quantity;
    }

    /// <summary>Повернення невикористаних запчастин (скасоване замовлення).</summary>
    public void Return(int quantity) => Restock(quantity);

    public override string GetSummary() =>
        $"{PartNumber} {Name} — {Price:0.00} грн, на складі {QuantityInStock} шт.";

    public override void Validate()
    {
        PartNumber = _partNumber;
        Name = _name;
        Price = _price;
        QuantityInStock = _quantityInStock;
        MinStockLevel = _minStockLevel;
    }

    public int CompareTo(SparePart? other) =>
        other is null ? 1 : string.Compare(Name, other.Name, StringComparison.CurrentCulture);
}
