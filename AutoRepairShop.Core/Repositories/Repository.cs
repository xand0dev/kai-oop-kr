using AutoRepairShop.Core.Exceptions;
using AutoRepairShop.Core.Interfaces;
using AutoRepairShop.Core.Models;

namespace AutoRepairShop.Core.Repositories;

/// <summary>
/// Узагальнений репозиторій у пам'яті на основі List&lt;T&gt;.
/// Призначає ідентифікатори та перевіряє записи перед додаванням.
/// </summary>
public class Repository<T> : IRepository<T> where T : EntityBase
{
    private readonly List<T> _items = new();
    private readonly string _entityName;
    private int _lastId;

    public Repository(string entityName)
    {
        _entityName = entityName;
    }

    public int Count => _items.Count;

    public T this[int id] => GetById(id);

    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        item.Validate();
        item.Id = ++_lastId;
        _items.Add(item);
    }

    public bool Remove(int id) => _items.RemoveAll(item => item.Id == id) > 0;

    public T GetById(int id) => FindById(id) ?? throw new EntityNotFoundException(_entityName, id);

    public T? FindById(int id) => _items.Find(item => item.Id == id);

    public IReadOnlyList<T> GetAll() => _items.AsReadOnly();

    public List<T> Find(Predicate<T> match) => _items.FindAll(match);

    public bool Exists(Predicate<T> match) => _items.Exists(match);

    public List<T> GetSorted(IComparer<T> comparer)
    {
        var sorted = new List<T>(_items);
        sorted.Sort(comparer);
        return sorted;
    }

    public void Load(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var loaded = new List<T>();
        var ids = new HashSet<int>();
        foreach (T item in items)
        {
            item.Validate();
            if (item.Id <= 0 || !ids.Add(item.Id))
                throw new ValidationException("ID", $"{_entityName}: некоректний або повторний ID {item.Id}.");
            loaded.Add(item);
        }

        _items.Clear();
        _items.AddRange(loaded);
        _lastId = loaded.Count == 0 ? 0 : loaded.Max(item => item.Id);
    }

    public void Clear()
    {
        _items.Clear();
        _lastId = 0;
    }
}
