using System.Text.RegularExpressions;
using AutoRepairShop.Core.Exceptions;

namespace AutoRepairShop.Core.Validation;

/// <summary>Допоміжні перевірки для сеттерів властивостей. Кожна кидає ValidationException.</summary>
internal static class Guard
{
    public static string Text(string? value, string property, int minLength, int maxLength)
    {
        string text = (value ?? string.Empty).Trim();
        if (text.Length < minLength || text.Length > maxLength)
            throw new ValidationException(property,
                $"{property}: довжина має бути від {minLength} до {maxLength} символів.");
        return text;
    }

    public static string Pattern(string? value, string property, Regex pattern, string hint)
    {
        string text = (value ?? string.Empty).Trim().ToUpperInvariant();
        if (!pattern.IsMatch(text))
            throw new ValidationException(property, $"{property}: очікується формат {hint}.");
        return text;
    }

    public static string Name(string? value, string property, int maxLength)
    {
        string text = Text(value, property, 2, maxLength);
        if (!ValidationRules.PersonName.IsMatch(text))
            throw new ValidationException(property, $"{property}: дозволені лише літери, пробіл, апостроф і дефіс.");
        return text;
    }

    public static int Range(int value, string property, int min, int max)
    {
        if (value < min || value > max)
            throw new ValidationException(property, $"{property}: значення має бути від {min} до {max}.");
        return value;
    }

    public static decimal Range(decimal value, string property, decimal min, decimal max)
    {
        if (value < min || value > max)
            throw new ValidationException(property, $"{property}: значення має бути від {min} до {max}.");
        return value;
    }

    public static T Defined<T>(T value, string property) where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw new ValidationException(property, $"{property}: невідоме значення {value}.");
        return value;
    }
}
