namespace AutoRepairShop.Core.Comparers;

/// <summary>
/// Власний абстрактний компаратор: реалізує стандартний IComparer&lt;T&gt;,
/// а нащадки описують лише сам критерій порівняння. Напрямок (зростання/спадання) задається в конструкторі.
/// </summary>
public abstract class DirectionalComparer<T> : IComparer<T> where T : class
{
    protected DirectionalComparer(bool descending)
    {
        Descending = descending;
    }

    public bool Descending { get; }

    public int Compare(T? x, T? y)
    {
        if (ReferenceEquals(x, y))
            return 0;
        if (x is null)
            return -1;
        if (y is null)
            return 1;
        int result = CompareItems(x, y);
        return Descending ? -result : result;
    }

    /// <summary>Порівняння двох ненульових об'єктів за зростанням.</summary>
    protected abstract int CompareItems(T x, T y);
}
