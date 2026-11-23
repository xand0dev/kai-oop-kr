using AutoRepairShop.Core.Exceptions;
using AutoRepairShop.Core.Models;
using AutoRepairShop.Core.Services;
using AutoRepairShop.Core.Validation;
using AutoRepairShop.UI.ConsoleTools;

namespace AutoRepairShop.UI.Menus;

/// <summary>Облік автомобілів клієнтів.</summary>
public sealed class CarsMenu : MenuBase
{
    public CarsMenu(AutoRepairShopService shop)
        : base(shop)
    {
    }

    protected override string Title => "Автомобілі";

    protected override IReadOnlyList<MenuItem> CreateItems() => new[]
    {
        new MenuItem("1", "Список автомобілів", ShowAll),
        new MenuItem("2", "Додати автомобіль", Add),
        new MenuItem("3", "Змінити власника / пробіг", Edit),
        new MenuItem("4", "Видалити автомобіль", Remove),
        new MenuItem("5", "Пошук (держномер, марка, власник)", Search),
        new MenuItem("6", "Сортувати за роком випуску", SortByYear),
        new MenuItem("7", "Історія ремонтів автомобіля", History),
    };

    private void ShowAll() => Print(Shop.Cars);

    private void Add()
    {
        string plate = ConsoleInput.ReadPattern("Держномер", ValidationRules.LicensePlate,
                                                ValidationRules.LicensePlateHint, toUpper: true);
        if (Shop.Cars.Any(c => c.LicensePlate == plate))
            throw new DuplicateEntityException("Автомобіль", plate); // не змушуємо вводити решту полів
        string make = ConsoleInput.ReadText("Марка", 2, 30);
        string model = ConsoleInput.ReadText("Модель", 1, 30);
        int year = ConsoleInput.ReadInt("Рік випуску", ValidationRules.MinCarYear, ValidationRules.MaxCarYear);
        string owner = ConsoleInput.ReadPattern("Власник (ПІБ)", ValidationRules.PersonName, "Іван Петренко");
        string phone = ConsoleInput.ReadPattern("Телефон", ValidationRules.Phone, ValidationRules.PhoneHint);
        int mileage = ConsoleInput.ReadInt("Пробіг, км", 0, ValidationRules.MaxMileageKm);

        Car car = Shop.AddCar(new Car(plate, make, model, year, owner, phone, mileage));
        ConsoleView.Success($"Додано: {car}");
    }

    private void Edit()
    {
        Car car = Shop.GetCar(ConsoleInput.ReadId("ID автомобіля"));
        ConsoleView.Info($"Поточні дані: {car.OwnerName}, {car.OwnerPhone}, {car.MileageKm} км");
        string owner = ConsoleInput.ReadPattern("Новий власник", ValidationRules.PersonName, "Іван Петренко");
        string phone = ConsoleInput.ReadPattern("Новий телефон", ValidationRules.Phone, ValidationRules.PhoneHint);
        int mileage = ConsoleInput.ReadInt("Новий пробіг, км", 0, ValidationRules.MaxMileageKm);
        Shop.UpdateCar(car.Id, owner, phone, mileage);
        ConsoleView.Success("Дані автомобіля оновлено.");
    }

    private void Remove()
    {
        Car car = Shop.GetCar(ConsoleInput.ReadId("ID автомобіля"));
        if (!ConsoleInput.Confirm($"Видалити {car.GetSummary()}?"))
            return;
        Shop.RemoveCar(car.Id);
        ConsoleView.Success("Автомобіль видалено.");
    }

    private void Search() => Print(Shop.FindCars(ConsoleInput.ReadText("Що шукати", 1, 40)));

    private void SortByYear() => Print(Shop.GetCarsSortedByYear(ConsoleInput.Confirm("Спочатку новіші?")));

    private void History()
    {
        Car car = Shop.GetCar(ConsoleInput.ReadId("ID автомобіля"));
        ConsoleView.Info(car.ToString());
        List<RepairOrder> history = Shop.GetCarHistory(car.Id);
        ConsoleView.Table(Rows.OrderHeaders, history.Select(o => Rows.Order(o, Shop)));
        ConsoleView.Info($"Усього витрачено на виконані ремонти: " +
                         $"{history.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.CalculateCost()):0.00} грн");
    }

    private static void Print(IEnumerable<Car> cars) => ConsoleView.Table(Rows.CarHeaders, cars.Select(Rows.Car));
}
