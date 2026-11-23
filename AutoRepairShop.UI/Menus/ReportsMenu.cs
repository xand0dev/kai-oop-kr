using AutoRepairShop.Core.Services;
using AutoRepairShop.UI.ConsoleTools;

namespace AutoRepairShop.UI.Menus;

/// <summary>Звіти: виручка, заробіток механіків, склад.</summary>
public sealed class ReportsMenu : MenuBase
{
    public ReportsMenu(AutoRepairShopService shop)
        : base(shop)
    {
    }

    protected override string Title => "Звіти";

    protected override IReadOnlyList<MenuItem> CreateItems() => new[]
    {
        new MenuItem("1", "Загальна статистика та виручка", ShowStatistics),
        new MenuItem("2", "Завантаженість і заробіток механіків", ShowMechanics),
        new MenuItem("3", "Стан складу", ShowStock),
    };

    private void ShowStatistics()
    {
        ShopStatistics stats = Shop.GetStatistics();
        Console.WriteLine($"Активних замовлень:     {stats.ActiveOrders}");
        Console.WriteLine($"Виконаних замовлень:    {stats.CompletedOrders}");
        Console.WriteLine($"Скасованих замовлень:   {stats.CancelledOrders}");
        Console.WriteLine($"Виручка за роботи:      {stats.LaborRevenue,12:0.00} грн");
        Console.WriteLine($"Виручка за запчастини:  {stats.PartsRevenue,12:0.00} грн");
        ConsoleView.Success($"Загальна виручка:       {stats.TotalRevenue,12:0.00} грн");
        Console.WriteLine($"Середній чек:           {stats.AverageCheck,12:0.00} грн");
    }

    private void ShowMechanics()
    {
        ConsoleView.Table(
            new[] { "ПІБ", "Активні", "Виконані", "Годин", "Заробіток, грн", "Стан" },
            Shop.GetMechanicReports().Select(r => (IReadOnlyList<string>)new[]
            {
                r.Mechanic.FullName, $"{r.ActiveOrders}/{r.Mechanic.MaxActiveOrders}", r.CompletedOrders.ToString(),
                r.HoursWorked.ToString("0.##"), r.LaborEarned.ToString("0.00"), r.IsBusy ? "зайнятий" : "вільний",
            }));
    }

    private void ShowStock()
    {
        Console.WriteLine($"Позицій на складі: {Shop.SpareParts.Count}");
        Console.WriteLine($"Вартість складу:   {Shop.GetStockValue():0.00} грн");
        ConsoleView.Warning("Закінчуються:");
        ConsoleView.Table(Rows.PartHeaders, Shop.GetLowStockParts().Select(Rows.Part));
    }
}
