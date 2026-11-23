namespace AutoRepairShop.Core.Models;

/// <summary>Українські назви значень перелічень для виведення в консоль.</summary>
public static class DisplayNames
{
    public static string ToUkrainian(this OrderStatus status) => status switch
    {
        OrderStatus.New => "Нове",
        OrderStatus.InProgress => "У роботі",
        OrderStatus.Completed => "Виконане",
        OrderStatus.Cancelled => "Скасоване",
        _ => status.ToString(),
    };

    public static string ToUkrainian(this Specialization specialization) => specialization switch
    {
        Specialization.Engine => "Двигун",
        Specialization.Transmission => "Трансмісія",
        Specialization.Suspension => "Ходова",
        Specialization.Electrical => "Електрика",
        Specialization.Bodywork => "Кузов",
        Specialization.Diagnostics => "Діагностика",
        _ => specialization.ToString(),
    };
}
