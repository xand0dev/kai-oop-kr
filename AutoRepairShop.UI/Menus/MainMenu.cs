using AutoRepairShop.Core.Services;
using AutoRepairShop.UI.ConsoleTools;

namespace AutoRepairShop.UI.Menus;

/// <summary>Головне меню застосунку.</summary>
public sealed class MainMenu : MenuBase
{
    public MainMenu(AutoRepairShopService shop)
        : base(shop)
    {
    }

    protected override string Title =>
        "СТО «Автосервіс» — головне меню" + (Shop.HasUnsavedChanges ? " (є незбережені зміни)" : string.Empty);

    protected override string BackCaption => "Вихід (зі збереженням)";

    protected override IReadOnlyList<MenuItem> CreateItems() => new[]
    {
        new MenuItem("1", "Автомобілі", () => new CarsMenu(Shop).Run()),
        new MenuItem("2", "Механіки", () => new MechanicsMenu(Shop).Run()),
        new MenuItem("3", "Склад запчастин", () => new PartsMenu(Shop).Run()),
        new MenuItem("4", "Замовлення на ремонт", () => new OrdersMenu(Shop).Run()),
        new MenuItem("5", "Звіти", () => new ReportsMenu(Shop).Run()),
        new MenuItem("6", "Зберегти зараз", Save),
        new MenuItem("7", "Завантажити демонстраційні дані", LoadDemo),
    };

    private void Save()
    {
        Shop.Save();
        ConsoleView.Success($"Дані збережено у {ConsoleView.ShortPath(Shop.StorageLocation)}");
    }

    private void LoadDemo()
    {
        if (Shop.Cars.Count > 0 && !ConsoleInput.Confirm("Поточні дані буде замінено демонстраційними. Продовжити?"))
            return;
        Shop.LoadDemoData();
        ConsoleView.Success("Завантажено: 4 автомобілі, 4 механіки, 8 запчастин, 3 замовлення.");
    }
}
