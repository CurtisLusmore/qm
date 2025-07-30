using System.Diagnostics.CodeAnalysis;

namespace qm.Repositories;

/// <summary>
/// A repository for storing items in insertion order
/// </summary>
/// <typeparam name="T">The type of item being stored</typeparam>
public class Repository<T>
    where T : class
{
    private readonly List<(string Key, T Item)> items = [];

    /// <summary>
    /// List all items in the repository
    /// </summary>
    /// <returns>An enumerable of the items in the repository</returns>
    public IEnumerable<T> List() => items.Select(item => item.Item).ToArray();

    /// <summary>
    /// Determine whether an item is in the repository
    /// </summary>
    /// <param name="key">The key of the item</param>
    /// <returns>Whether the item is in the repository</returns>
    public bool Contains(string key) => items.Any(item => item.Key == key);

    /// <summary>
    /// Try to get an item from the repository
    /// </summary>
    /// <param name="key">The key of the item</param>
    /// <param name="item">The item, if present</param>
    /// <returns>Whether the item is in the repository</returns>
    public bool TryGet(string key, [NotNullWhen(true)] out T? item)
    {
        var pair = items.SingleOrDefault(item => item.Key == key);
        if (pair.Key is null)
        {
            item = default;
            return false;
        }
        item = pair.Item;
        return true;
    }

    /// <summary>
    /// Add an item to the repository
    /// </summary>
    /// <param name="key">The key of the item</param>
    /// <param name="item">The item</param>
    public void Add(string key, T item)
    {
        items.Add((key, item));
    }

    /// <summary>
    /// Remove an item from the repository
    /// </summary>
    /// <param name="key">The key of the item</param>
    public void Remove(string key)
    {
        var pair = items.SingleOrDefault(item => item.Key == key);
        items.Remove(pair);
    }
}
