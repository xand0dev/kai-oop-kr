using AutoRepairShop.Core.Models;
using AutoRepairShop.Core.Services;
using AutoRepairShop.Core.Validation;
using AutoRepairShop.UI.ConsoleTools;

namespace AutoRepairShop.UI.Menus;

/// <summary>Облік механіків СТО.</summary>
public sealed class MechanicsMenu : MenuBase
{
    public MechanicsMenu(AutoRepairShopService shop)
        : base(shop)
    {
    }

    protected override string Title => "Механіки";

    protected override IReadOnlyList<MenuItem> CreateItems() => new[]
    {
        new MenuItem("1", "Список механіків", () => Print(Shop.Mechanics)),
        new MenuItem("2", "Додати механіка", Add),
        new MenuItem("3", "Змінити ставку / максимум замовлень", Edit),
        new MenuItem("4", "Видалити механіка", Remove),
        new MenuItem("5", "Фільтр за спеціалізацією", FilterBySpecialization),
        new MenuItem("6", "Лише вільні механіки", () => Print(Shop.GetAvailableMechanics())),
        new MenuItem("7", "Сортувати за ставкою", SortByRate),
    };

    private void Add()
    {
        string name = ConsoleInput.ReadPattern("ПІБ", ValidationRules.PersonName, "Сергій Мельник");
        Specialization specialization = ConsoleInput.ReadEnum<Specialization>("Спеціалізація", s => s.ToUkrainian());
        decimal rate = ConsoleInput.ReadDecimal("Ставка, грн/год", ValidationRules.MinHourlyRate,
                                                ValidationRules.MaxHourlyRate);
        int max = ConsoleInput.ReadInt("Максимум одночасних замовлень", 1, ValidationRules.MaxActiveOrdersLimit);
        Mechanic mechanic = Shop.AddMechanic(new Mechanic(name, specialization, rate, max));
        ConsoleView.Success($"Додано: {mechanic}");
    }

    private void Edit()
    {
        Mechanic mechanic = Shop.GetMechanic(ConsoleInput.ReadId("ID механіка"));
        ConsoleView.Info($"Зараз: {mechanic.HourlyRate:0.00} грн/год, максимум {mechanic.MaxActiveOrders}");
        decimal rate = ConsoleInput.ReadDecimal("Нова ставка, грн/год", ValidationRules.MinHourlyRate,
                                                ValidationRules.MaxHourlyRate);
        int max = ConsoleInput.ReadInt("Новий максимум замовлень", 1, ValidationRules.MaxActiveOrdersLimit);
        Shop.UpdateMechanic(mechanic.Id, rate, max);
        ConsoleView.Success("Дані механіка оновлено. Ставка в уже доданих роботах не змінюється.");
    }

    private void Remove()
    {
        Mechanic mechanic = Shop.GetMechanic(ConsoleInput.ReadId("ID механіка"));
        if (!ConsoleInput.Confirm($"Видалити {mechanic.FullName}?"))
            return;
        Shop.RemoveMechanic(mechanic.Id);
        ConsoleView.Success("Механіка видалено.");
    }

    private void FilterBySpecialization()
    {
        Specialization specialization = ConsoleInput.ReadEnum<Specialization>("Спеціалізація", s => s.ToUkrainian());
        Print(Shop.GetMechanicsBySpecialization(specialization));
    }

    private void SortByRate() => Print(Shop.GetMechanicsSortedByRate(ConsoleInput.Confirm("Спочатку найдорожчі?")));

    private void Print(IEnumerable<Mechanic> mechanics) =>
        ConsoleView.Table(Rows.MechanicHeaders, mechanics.Select(m => Rows.Mechanic(m, Shop)));
}
