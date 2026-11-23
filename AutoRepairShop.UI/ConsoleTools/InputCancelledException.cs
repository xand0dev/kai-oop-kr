namespace AutoRepairShop.UI.ConsoleTools;

/// <summary>Користувач перервав введення (порожній рядок у полі вибору або кінець потоку вводу).</summary>
public class InputCancelledException : Exception
{
    public InputCancelledException()
        : base("Введення скасовано.")
    {
    }
}
