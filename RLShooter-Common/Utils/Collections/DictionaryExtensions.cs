namespace RLShooter.Common.Utils.Collections;

public static class DictionaryExtensions
{
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory) {
        if (dictionary.TryGetValue(key, out var value)) {
            return value;
        }

        value = valueFactory();
        dictionary[key] = value;
        return value;
    }
    
    public static List<T> GetOrAdd<TKey, T>(this Dictionary<TKey, List<T>> dictionary, TKey key) {
        if (dictionary.TryGetValue(key, out var value)) {
            return value;
        }

        value           = [];
        dictionary[key] = value;
        return value;
    }
    public static void Increment<TKey>(this Dictionary<TKey, int> dictionary, TKey key) {
        if (dictionary.TryGetValue(key, out var value)) {
            dictionary[key] = value + 1;
        } else {
            dictionary[key] = 1;
        }
    }
}