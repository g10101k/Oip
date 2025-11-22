using System.Collections;
using System.Globalization;
using System.Reflection;
using Oip.Settings.Attributes;

namespace Oip.Settings.Helpers;

/// <summary>
/// Provides static methods for converting application settings instances to dictionaries
/// </summary>
public static class Flatter
{
    /// <summary>
    /// Convert application settings instance to dictionary
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="obj"></param>
    /// <param name="prefix"></param>
    public static void ToDictionary(Dictionary<string, string> dictionary, object obj, string prefix)
    {
        var fields = obj.GetType().GetProperties();
        foreach (var field in fields)
        {
            if (field.GetCustomAttribute(typeof(NotSaveToDbAttribute)) != null)
                continue;

            var value = field.GetValue(obj);
            if (value == null) continue;

            var key = string.IsNullOrEmpty(prefix) ? field.Name : string.Join(':', prefix, field.Name);

            if (IsSimpleOrNull(value))
            {
                dictionary.Add(key, ToStringInvariant(value));
            }
            else if (value.GetType().IsGenericType)
            {
                GenericToDictionary(dictionary, prefix, value, key);
            }
            else
            {
                ToDictionary(dictionary, value, key);
            }
        }
    }


    /// <summary>
    /// Generic type to dictionary
    /// </summary>
    /// <param name="dictionary"></param>
    /// <param name="prefix"></param>
    /// <param name="value"></param>
    /// <param name="key"></param>
    private static void GenericToDictionary(Dictionary<string, string> dictionary, string prefix, object value,
        string key)
    {
        if (value.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            if (value is IDictionary dictionary1)
            {
                foreach (DictionaryEntry keyValue in dictionary1)
                {
                    var key2 = $"{prefix}{key}:{keyValue.Key}";
                    if (IsSimpleOrNull(keyValue.Value))
                    {
                        dictionary.Add(key2, ToStringInvariant(keyValue.Value));
                    }
                    else
                    {
                        ToDictionary(dictionary, keyValue.Value!, key2);
                    }
                }
            }
        }
        else if (value.GetType().GetGenericTypeDefinition() == typeof(List<>))
        {
            var i = 0;
            foreach (var item in (IEnumerable)value)
            {
                if (IsSimpleOrNull(item))
                {
                    dictionary.Add($"{key}:{i}", ToStringInvariant(item));
                }
                else
                {
                    ToDictionary(dictionary, item, $"{key}:{i}");
                }

                i++;
            }
        }
    }

    private static string ToStringInvariant(object? obj)
    {
        return obj switch
        {
            null => string.Empty,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => obj.ToString()!
        };
    }

    private static bool IsSimpleOrNull(object? obj)
    {
        if (obj is null) return true;
        var type = obj.GetType();
        return type.IsPrimitive ||
               type.IsEnum ||
               type == typeof(string) ||
               type == typeof(decimal) ||
               type == typeof(DateTime) ||
               type == typeof(DateTimeOffset) ||
               type == typeof(TimeSpan) ||
               type == typeof(Guid);
    }
}