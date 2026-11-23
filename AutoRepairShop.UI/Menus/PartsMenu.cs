using AutoRepairShop.Core.Exceptions;
using AutoRepairShop.Core.Models;
using AutoRepairShop.Core.Services;
using AutoRepairShop.Core.Validation;
using AutoRepairShop.UI.ConsoleTools;

namespace AutoRepairShop.UI.Menus;

/// <summary>Склад запчастин.</summary>
public sealed class PartsMenu : MenuBase
{
    public PartsMenu(AutoRepairShopService shop)
        : base(shop)
    {
    }

    protected override string Title => "Склад запчастин";

    protected override IReadOnlyList<MenuItem> CreateItems() => new[]
    {
        new MenuItem("1", "Список запчастин", () => Print(Shop.SpareParts)),
        new MenuItem("2", "Додати запчастину", Add),
        new MenuItem("3", "Поповнити склад", Restock),
        new MenuItem("4", "Змінити ціну", ChangePrice),
        new MenuItem("5", "Видалити запчастину", Remove),
        new MenuItem("6", "Пошук (артикул, назва)", () => Print(Shop.FindSpareParts(ConsoleInput.ReadText("Що шукати", 1, 40)))),
        new MenuItem("7", "Сортувати за ціною", () => Print(Shop.GetPartsSortedByPrice(ConsoleInput.Confirm("Спочатку найдорожчі?")))),
        new MenuItem("8", "Закінчуються (залишок ≤ мінімуму)", () => Print(Shop.GetLowStockParts())),
    };

    private void Add()
    {
        string number = ConsoleInput.ReadPattern("Артикул", ValidationRules.PartNumber,
                                                 ValidationRules.PartNumberHint, toUpper: true);
        if (Shop.SpareParts.Any(p => p.PartNumber == number))
            throw new DuplicateEntityException("Запчастина", number);
        string name = ConsoleInput.ReadText("Назва", 2, 60);
        decimal price = ConsoleInput.ReadDecimal("Ціна, грн", 0.01m, ValidationRules.MaxPartPrice);
        int quantity = ConsoleInput.ReadInt("Кількість на складі", 0, ValidationRules.MaxStockQuantity);
        int min = ConsoleInput.ReadInt("Мінімальний залишок", 0, 1_000);
        SparePart part = Shop.AddSparePart(new SparePart(number, name, price, quantity, min));
        ConsoleView.Success($"Додано: {part}");
    }

    private void Restock()
    {
        SparePart part = Shop.GetSparePart(ConsoleInput.ReadId("ID запчастини"));
        int quantity = ConsoleInput.ReadInt($"Скільки додати до {part.QuantityInStock} шт.", 1, 10_000);
        Shop.RestockPart(part.Id, quantity);
        ConsoleView.Success($"Тепер на складі: {part.QuantityInStock} шт.");
    }

    private void ChangePrice()
    {
        SparePart part = Shop.GetSparePart(ConsoleInput.ReadId("ID запчастини"));
        decimal price = ConsoleInput.ReadDecimal($"Нова ціна (зараз {part.Price:0.00})", 0.01m, ValidationRules.MaxPartPrice);
        Shop.UpdatePartPrice(part.Id, price);
        ConsoleView.Success("Ціну змінено. У вже списаних запчастинах ціна лишається попередньою.");
    }

    private void Remove()
    {
        SparePart part = Shop.GetSparePart(ConsoleInput.ReadId("ID запчастини"));
        if (!ConsoleInput.Confirm($"Видалити {part.Name}?"))
            return;
        Shop.RemoveSparePart(part.Id);
        ConsoleView.Success("Запчастину видалено.");
    }

    private void Print(IEnumerable<SparePart> parts)
    {
        List<SparePart> list = parts.ToList();
        ConsoleView.Table(Rows.PartHeaders, list.Select(Rows.Part));
        if (list.Count > 0)
            ConsoleView.Info($"Вартість показаних запчастин: {list.Sum(p => p.Price * p.QuantityInStock):0.00} грн");
    }
}
