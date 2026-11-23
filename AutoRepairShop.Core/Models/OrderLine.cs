using AutoRepairShop.Core.Interfaces;

namespace AutoRepairShop.Core.Models;

/// <summary>
/// Абстрактний рядок замовлення: виконана робота або використана запчастина.
/// Кожен нащадок по-своєму обчислює вартість (поліморфізм).
/// </summary>
public abstract class OrderLine : ICostCalculable
{
    /// <summary>Текст рядка для чека.</summary>
    public abstract string Describe();

    public abstract decimal CalculateCost();

    public override string ToString() => $"{Describe()} = {CalculateCost():0.00} грн";
}
