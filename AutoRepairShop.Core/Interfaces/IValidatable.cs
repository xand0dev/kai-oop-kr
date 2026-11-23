namespace AutoRepairShop.Core.Interfaces;

/// <summary>Об'єкт, який уміє перевірити власну коректність (наприклад, після завантаження з файла).</summary>
public interface IValidatable
{
    /// <summary>Кидає ValidationException, якщо стан об'єкта некоректний.</summary>
    void Validate();
}
