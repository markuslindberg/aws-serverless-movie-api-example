namespace MovieApi.Extensions;

public static class DictionaryExtensions
{
    public static TValue? GetValueOrDefault<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue? defaultValue = default(TValue))
        where TKey : notnull
    {
        if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
        if (key == null) throw new ArgumentNullException(nameof(key));

        TValue? value;
        return dictionary.TryGetValue(key, out value) ? value : defaultValue;
    }
}