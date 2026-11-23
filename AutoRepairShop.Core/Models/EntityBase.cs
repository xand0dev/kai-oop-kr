using System.Text.Json.Serialization;
using AutoRepairShop.Core.Interfaces;

namespace AutoRepairShop.Core.Models;

/// <summary>
/// Абстрактна база для всіх сутностей: ідентифікатор, перевірка коректності та короткий опис.
/// Ідентифікатор призначає лише репозиторій (internal set).
/// </summary>
public abstract class EntityBase : IEntity, IValidatable
{
    [JsonInclude]
    public int Id { get; internal set; }

    /// <summary>Назва типу сутності для повідомлень («Автомобіль», «Механік»...).</summary>
    [JsonIgnore]
    public abstract string EntityName { get; }

    /// <summary>Короткий опис для списків і повідомлень.</summary>
    public abstract string GetSummary();

    public abstract void Validate();

    public override string ToString() => $"#{Id} {GetSummary()}";
}
