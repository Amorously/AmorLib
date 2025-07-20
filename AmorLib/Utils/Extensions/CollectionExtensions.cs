namespace AmorLib.Utils.Extensions;

public static class CollectionExtensions
{
    /// <summary>
    /// Converts a <see cref="Il2CppSystem.Collections.Generic.List{T}"/> to <see cref="List{T}"/>.
    /// </summary>
    /// <remarks>(Borrowed from GTFO-API until it is updated.)</remarks>
    /// <returns>A copy of the list as <see cref="List{T}"/>.</returns>
    public static List<T> ToManaged<T>(this Il2CppSystem.Collections.Generic.List<T> list)
    {
        List<T> managedList = new(list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            managedList.Add(list[i]);
        }
        return managedList;
    }

    /// <summary>
    /// Creates a new default <typeparamref name="TValue"/> if the key doesn't exist and returns the value.
    /// </summary>
    /// <remarks>
    /// If <typeparamref name="TKey"/> does not exist, it is added to the dictionary with a new <typeparamref name="TValue"/> instance.
    /// </remarks>
    /// <returns>The value mapped to an existing key, or a new instance of <typeparamref name="TValue"/> if the key was not found.</returns>
    public static TValue GetOrAddNew<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
    {
        if (!dict.TryGetValue(key, out TValue? value))
        {
            value = new();
            dict[key] = value;
        }
        return value;
    }

    /// <summary>
    /// Invokes the <paramref name="action"/> on each value in the dictionary.
    /// </summary>
    public static void ForEachValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, Action<TValue> action) where TKey : notnull
    {
        foreach (var value in dict.Values)
        {
            action(value);
        }
    }

    /// <summary>
    /// Detemines whether any nested <typeparamref name="TCollection"/> in the dictionary meet the given condition(s).
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if any collection contains one <typeparamref name="TValue"/> which satisfies the <paramref name="predicate"/>.
    /// </returns>
    public static bool AnyAllValues<TKey, TCollection, TValue>(this IDictionary<TKey, TCollection> dict, Func<TValue, bool> predicate) where TCollection : IEnumerable<TValue>
    {
        foreach (var values in dict.Values)
        {
            if (values.Any(predicate))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Searches all <typeparamref name="TCollection"/> in the dictionary for the first <typeparamref name="TValue"/> which meet the given condition(s).
    /// </summary>
    /// <returns>
    /// The first <typeparamref name="TValue"/> which satisfies the <paramref name="predicate"/>; or <see langword="default"/> if nothing is found.
    /// </returns>
    public static TValue? FirstOrDefaultAllValues<TKey, TCollection, TValue>(this IDictionary<TKey, TCollection> dict, Func<TValue, bool> predicate) where TCollection : IEnumerable<TValue>
    {
        foreach (var collection in dict.Values)
        {
            foreach (var item in collection)
            {
                if (predicate(item))
                {
                    return item;
                }
            }
        }
        return default;
    }
}
