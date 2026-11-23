using System.Text;
using AutoRepairShop.Core.Exceptions;
using AutoRepairShop.Core.Services;
using AutoRepairShop.Core.Storage;
using AutoRepairShop.UI.ConsoleTools;
using AutoRepairShop.UI.Menus;

namespace AutoRepairShop.UI;

/// <summary>
/// Точка входу. Аргументи командного рядка:
///   --data &lt;шлях&gt;  файл даних (за замовчуванням data/auto-repair-shop.json);
///   --demo          почати з демонстраційних даних, якщо файла ще немає;
///   --help          довідка.
/// </summary>
public static class Program
{
    private const string DefaultDataFile = "data/auto-repair-shop.json";

    public static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        if (args.Contains("--help") || args.Contains("-h"))
        {
            PrintHelp();
            return 0;
        }

        string dataFile = GetOption(args, "--data") ?? DefaultDataFile;
        var shop = new AutoRepairShopService(new JsonDataStorage(dataFile));

        if (!LoadData(shop, args.Contains("--demo")))
            return 1;

        // Ctrl+C теж зберігає дані перед завершенням.
        Console.CancelKeyPress += (_, _) => SaveData(shop);

        new MainMenu(shop).Run();
        return SaveData(shop) ? 0 : 1;
    }

    /// <summary>Завантаження на старті; пошкоджений файл не перезаписується без згоди користувача.</summary>
    private static bool LoadData(AutoRepairShopService shop, bool demo)
    {
        try
        {
            if (shop.Load())
            {
                ConsoleView.Success($"Дані завантажено з {ConsoleView.ShortPath(shop.StorageLocation)}: автомобілів {shop.Cars.Count}, " +
                                    $"механіків {shop.Mechanics.Count}, запчастин {shop.SpareParts.Count}, " +
                                    $"замовлень {shop.Orders.Count}.");
            }
            else if (demo)
            {
                shop.LoadDemoData();
                ConsoleView.Info("Файла даних ще немає — завантажено демонстраційні дані.");
            }
            else
            {
                ConsoleView.Info($"Файла {ConsoleView.ShortPath(shop.StorageLocation)} ще немає — починаємо з порожньої бази.");
            }

            return true;
        }
        catch (DataStorageException ex)
        {
            ConsoleView.Error(ex.Message);
            try
            {
                if (!ConsoleInput.Confirm("Почати з порожньої бази? Копію пошкодженого файла буде збережено як .bak"))
                    return false;
            }
            catch (InputCancelledException)
            {
                return false;
            }

            File.Copy(shop.StorageLocation, shop.StorageLocation + ".bak", overwrite: true);
            ConsoleView.Warning($"Копію збережено: {ConsoleView.ShortPath(shop.StorageLocation)}.bak");
            return true;
        }
    }

    private static bool SaveData(AutoRepairShopService shop)
    {
        try
        {
            shop.Save();
            ConsoleView.Success($"Дані збережено у {ConsoleView.ShortPath(shop.StorageLocation)}. До побачення!");
            return true;
        }
        catch (DataStorageException ex)
        {
            ConsoleView.Error(ex.Message);
            return false;
        }
    }

    private static string? GetOption(string[] args, string name)
    {
        int index = Array.IndexOf(args, name);
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Облік автосервісу (СТО). Курсова робота з ООП, варіант 22.");
        Console.WriteLine("Використання: AutoRepairShop [--data <файл.json>] [--demo]");
        Console.WriteLine("  --data <файл>  файл даних JSON (за замовчуванням data/auto-repair-shop.json)");
        Console.WriteLine("  --demo         якщо файла немає, почати з демонстраційних даних");
    }
}
