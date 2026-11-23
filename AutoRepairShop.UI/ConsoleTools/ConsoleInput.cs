using System.Globalization;
using System.Text.RegularExpressions;

namespace AutoRepairShop.UI.ConsoleTools;

/// <summary>
/// Контроль коректності введених даних: кожен метод повторює запит, доки значення не стане правильним.
/// Порожній рядок у полях ID та текстових полях скасовує операцію.
/// </summary>
public static class ConsoleInput
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public static string ReadLine(string prompt)
    {
        Console.Write(prompt);
        string? line = Console.ReadLine();
        if (line is null)
            throw new InputCancelledException(); // кінець вводу (Ctrl+D / перенаправлений файл)
        return line.Trim();
    }

    public static int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            string text = ReadLine($"{prompt} [{min}..{max}]: ");
            if (int.TryParse(text, NumberStyles.Integer, Invariant, out int value) && value >= min && value <= max)
                return value;
            ConsoleView.Error($"Введіть ціле число від {min} до {max}.");
        }
    }

    /// <summary>Читає ID; порожній рядок — скасування.</summary>
    public static int ReadId(string prompt)
    {
        while (true)
        {
            string text = ReadLine($"{prompt} (Enter — скасувати): ");
            if (text.Length == 0)
                throw new InputCancelledException();
            if (int.TryParse(text, NumberStyles.Integer, Invariant, out int id) && id > 0)
                return id;
            ConsoleView.Error("ID — додатне ціле число.");
        }
    }

    /// <summary>Десяткове число; приймає і крапку, і кому як роздільник.</summary>
    public static decimal ReadDecimal(string prompt, decimal min, decimal max)
    {
        while (true)
        {
            string text = ReadLine($"{prompt} (від {min} до {max}): ").Replace(',', '.');
            if (decimal.TryParse(text, NumberStyles.Number, Invariant, out decimal value)
                && value >= min && value <= max && decimal.Round(value, 2) == value)
                return value;
            ConsoleView.Error($"Введіть число від {min} до {max} (не більше двох знаків після коми).");
        }
    }

    public static string ReadText(string prompt, int minLength, int maxLength)
    {
        while (true)
        {
            string text = ReadLine($"{prompt}: ");
            if (text.Length == 0)
                throw new InputCancelledException(); // порожній рядок — скасування операції
            if (text.Length >= minLength && text.Length <= maxLength)
                return text;
            ConsoleView.Error($"Довжина має бути від {minLength} до {maxLength} символів.");
        }
    }

    /// <summary>Рядок за регулярним виразом; перед перевіркою переводиться у верхній регістр, якщо треба.</summary>
    public static string ReadPattern(string prompt, Regex pattern, string hint, bool toUpper = false)
    {
        while (true)
        {
            string text = ReadLine($"{prompt} ({hint}): ");
            if (text.Length == 0)
                throw new InputCancelledException();
            if (toUpper)
                text = text.ToUpperInvariant();
            if (pattern.IsMatch(text))
                return text;
            ConsoleView.Error($"Невірний формат. Приклад: {hint}.");
        }
    }

    public static TEnum ReadEnum<TEnum>(string prompt, Func<TEnum, string> displayName) where TEnum : struct, Enum
    {
        TEnum[] values = Enum.GetValues<TEnum>();
        for (int i = 0; i < values.Length; i++)
            Console.WriteLine($"  {i + 1}. {displayName(values[i])}");
        return values[ReadInt(prompt, 1, values.Length) - 1];
    }

    public static bool Confirm(string question)
    {
        while (true)
        {
            string answer = ReadLine($"{question} (т/н): ").ToLowerInvariant();
            if (answer is "т" or "так" or "y" or "yes")
                return true;
            if (answer is "н" or "ні" or "n" or "no")
                return false;
            ConsoleView.Error("Відповідайте «т» або «н».");
        }
    }
}
