namespace AutoRepairShop.UI.ConsoleTools;

/// <summary>Кольорове виведення повідомлень і таблиць у консоль.</summary>
public static class ConsoleView
{
    public static void Title(string text)
    {
        Console.WriteLine();
        WriteColored($"══ {text} ══", ConsoleColor.Cyan);
    }

    public static void Success(string text) => WriteColored("✔ " + text, ConsoleColor.Green);

    public static void Error(string text) => WriteColored("✖ " + text, ConsoleColor.Red);

    public static void Warning(string text) => WriteColored("! " + text, ConsoleColor.Yellow);

    public static void Info(string text) => WriteColored(text, ConsoleColor.Gray);

    /// <summary>Шлях відносно поточної папки — коротше для виведення.</summary>
    public static string ShortPath(string path) => Path.GetRelativePath(Environment.CurrentDirectory, path);

    /// <summary>Таблиця з автоматичною шириною стовпців.</summary>
    public static void Table(IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<string>> rows)
    {
        List<IReadOnlyList<string>> data = rows.ToList();
        if (data.Count == 0)
        {
            Warning("Записів не знайдено.");
            return;
        }

        int[] widths = headers.Select(h => h.Length).ToArray();
        foreach (IReadOnlyList<string> row in data)
            for (int i = 0; i < widths.Length; i++)
                widths[i] = Math.Max(widths[i], row[i].Length);

        string separator = "+" + string.Join("+", widths.Select(w => new string('-', w + 2))) + "+";
        Console.WriteLine(separator);
        WriteColored(FormatRow(headers, widths), ConsoleColor.White);
        Console.WriteLine(separator);
        foreach (IReadOnlyList<string> row in data)
            Console.WriteLine(FormatRow(row, widths));
        Console.WriteLine(separator);
        Info($"Усього: {data.Count}");
    }

    private static string FormatRow(IReadOnlyList<string> cells, int[] widths) =>
        "| " + string.Join(" | ", cells.Select((cell, i) => cell.PadRight(widths[i]))) + " |";

    private static void WriteColored(string text, ConsoleColor color)
    {
        ConsoleColor previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = previous;
    }
}
