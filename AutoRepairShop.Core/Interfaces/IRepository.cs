namespace AutoRepairShop.Core.Interfaces;

/// <summary>Елементарні операції з набором сутностей одного типу.</summary>
/// <typeparam name="T">Тип сутності.</typeparam>
public interface IRepository<T> where T : class, IEntity
{
    /// <summary>Кількість записів.</summary>
    int Count { get; }

    /// <summary>Доступ до запису за ідентифікатором (індексатор).</summary>
    T this[int id] { get; }

    void Add(T item);
    bool Remove(int id);
    T GetById(int id);
    T? FindById(int id);
    IReadOnlyList<T> GetAll();
    List<T> Find(Predicate<T> match);
    bool Exists(Predicate<T> match);
    List<T> GetSorted(IComparer<T> comparer);

    /// <summary>Замінює вміст репозиторію записами, завантаженими з файла.</summary>
    void Load(IEnumerable<T> items);
    void Clear();
}
