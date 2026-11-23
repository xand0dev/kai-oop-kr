using AutoRepairShop.Core.Exceptions;
using AutoRepairShop.Core.Services;
using AutoRepairShop.UI.ConsoleTools;

namespace AutoRepairShop.UI.Menus;

/// <summary>
/// Абстрактне меню: цикл показу пунктів, вибір і централізована обробка винятків.
/// Нащадки лише описують свої пункти в CreateItems.
/// </summary>
public abstract class MenuBase
{
    private const string BackKey = "0";

    protected MenuBase(AutoRepairShopService shop)
    {
        Shop = shop;
    }

    protected AutoRepairShopService Shop { get; }

    protected abstract string Title { get; }

    /// <summary>Підпис пункту «0» (у головному меню — «Вихід»).</summary>
    protected virtual string BackCaption => "Назад";

    protected abstract IReadOnlyList<MenuItem> CreateItems();

    public void Run()
    {
        IReadOnlyList<MenuItem> items = CreateItems();
        while (true)
        {
            ConsoleView.Title(Title);
            foreach (MenuItem item in items)
                Console.WriteLine($"  {item.Key,2}. {item.Caption}");
            Console.WriteLine($"  {BackKey,2}. {BackCaption}");

            string choice;
            try
            {
                choice = ConsoleInput.ReadLine("Ваш вибір: ");
            }
            catch (InputCancelledException)
            {
                return; // кінець вводу — виходимо з меню
            }

            if (choice == BackKey)
                return;
            MenuItem? selected = items.FirstOrDefault(i => i.Key == choice);
            if (selected is null)
            {
                ConsoleView.Error("Немає такого пункту меню.");
                continue;
            }

            Execute(selected);
        }
    }

    /// <summary>Виконання дії з перехопленням винятків, щоб помилка не завершила програму.</summary>
    private static void Execute(MenuItem item)
    {
        try
        {
            item.Action();
        }
        catch (InputCancelledException)
        {
            ConsoleView.Warning("Операцію скасовано.");
        }
        catch (PartOutOfStockException ex)
        {
            ConsoleView.Error("Склад: " + ex.Message);
        }
        catch (MechanicBusyException ex)
        {
            ConsoleView.Error("Призначення неможливе. " + ex.Message);
        }
        catch (ValidationException ex)
        {
            ConsoleView.Error($"Некоректні дані ({ex.PropertyName}): {ex.Message}");
        }
        catch (DataStorageException ex)
        {
            ConsoleView.Error("Файл: " + ex.Message);
        }
        catch (AutoRepairShopException ex)
        {
            ConsoleView.Error(ex.Message);
        }
    }
}
