namespace AutoRepairShop.Core.Exceptions;

/// <summary>Запчастини немає на складі в потрібній кількості (виняток варіанта 22).</summary>
public class PartOutOfStockException : AutoRepairShopException
{
    public PartOutOfStockException(string partNumber, string partName, int requested, int available)
        : base($"Запчастина {partNumber} «{partName}» відсутня на складі в потрібній кількості: " +
               $"потрібно {requested} шт., є {available} шт.")
    {
        PartNumber = partNumber;
        Requested = requested;
        Available = available;
    }

    public string PartNumber { get; }
    public int Requested { get; }
    public int Available { get; }
}
