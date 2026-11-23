using AutoRepairShop.Core.Models;

namespace AutoRepairShop.Core.Services;

/// <summary>Демонстраційні дані: заповнюються через звичайні методи сервісу, тож проходять усі перевірки.</summary>
internal static class DemoData
{
    public static void Fill(AutoRepairShopService shop)
    {
        Car golf = shop.AddCar(new Car("AA1234BB", "Volkswagen", "Golf", 2015, "Іван Петренко", "+380671234567", 182_000));
        Car camry = shop.AddCar(new Car("KA5678CE", "Toyota", "Camry", 2019, "Олена Коваль", "+380501112233", 96_500));
        Car focus = shop.AddCar(new Car("BC0007AX", "Ford", "Focus", 2012, "Андрій Шевчук", "+380931234000", 240_300));
        shop.AddCar(new Car("AI4321KM", "Skoda", "Octavia", 2021, "Марія Бондар", "+380661230987", 41_200));

        Mechanic engine = shop.AddMechanic(new Mechanic("Сергій Мельник", Specialization.Engine, 650m, 2));
        Mechanic suspension = shop.AddMechanic(new Mechanic("Олег Ткаченко", Specialization.Suspension, 500m, 1));
        shop.AddMechanic(new Mechanic("Віктор Гнатюк", Specialization.Electrical, 600m, 2));
        shop.AddMechanic(new Mechanic("Дмитро Лисенко", Specialization.Diagnostics, 450m, 3));

        SparePart oilFilter = shop.AddSparePart(new SparePart("FLT-0101", "Масляний фільтр", 320m, 15, 5));
        SparePart oil = shop.AddSparePart(new SparePart("OIL-5030", "Моторна олива 5W-30, 1 л", 410m, 40, 10));
        SparePart pads = shop.AddSparePart(new SparePart("BRK-2001", "Гальмівні колодки передні", 1_450m, 6, 2));
        shop.AddSparePart(new SparePart("BRK-2002", "Гальмівний диск передній", 2_300m, 4, 2));
        SparePart stabilizer = shop.AddSparePart(new SparePart("SUS-3010", "Стійка стабілізатора", 780m, 3, 2));
        shop.AddSparePart(new SparePart("ELC-4005", "Свічка запалювання", 260m, 24, 8));
        shop.AddSparePart(new SparePart("ELC-4100", "Акумулятор 74 А·год", 4_200m, 1, 1));
        shop.AddSparePart(new SparePart("BLT-5001", "Ремінь ГРМ (комплект)", 5_600m, 2, 1));

        // Виконане замовлення: заміна оливи та фільтра.
        RepairOrder service = shop.CreateOrder(golf.Id, "Планове ТО: заміна оливи та фільтра");
        shop.AssignMechanic(service.Id, engine.Id);
        shop.AddWork(service.Id, "Заміна моторної оливи та фільтра", 1.0m);
        shop.AddPartToOrder(service.Id, oilFilter.Id, 1);
        shop.AddPartToOrder(service.Id, oil.Id, 5);
        shop.CompleteOrder(service.Id);

        // Активне замовлення: Олег Ткаченко (максимум 1 замовлення) тепер зайнятий.
        RepairOrder brakes = shop.CreateOrder(focus.Id, "Стукіт у підвісці, скрип гальм");
        shop.AssignMechanic(brakes.Id, suspension.Id);
        shop.AddWork(brakes.Id, "Заміна стійок стабілізатора", 1.5m);
        shop.AddWork(brakes.Id, "Заміна передніх гальмівних колодок", 1.0m);
        shop.AddPartToOrder(brakes.Id, stabilizer.Id, 2);
        shop.AddPartToOrder(brakes.Id, pads.Id, 1);

        // Нове замовлення без механіка.
        shop.CreateOrder(camry.Id, "Горить Check Engine, нерівна робота двигуна");
    }
}
