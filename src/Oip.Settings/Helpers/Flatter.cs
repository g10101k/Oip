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
    /// Converts an application settings instance to a flat dictionary
    /// </summary>
    /// <param name="dictionary">Target dictionary to populate with flattened data</param>
    /// <param name="obj">Object instance to flatten</param>
    /// <param name="prefix">Key prefix for nested properties</param>
    public static Dictionary<string, string> ToDictionary(object obj, string prefix)
    {
        var visitedObjects = new HashSet<object>();
        var dictionary = new Dictionary<string, string>();
        ToDictionaryInternal(dictionary, obj, prefix, visitedObjects);
        return dictionary;
    }

    /// <summary>
    /// Internal recursive method with cycle detection for flattening objects
    /// </summary>
    /// <param name="dictionary">Target dictionary to populate</param>
    /// <param name="obj">Current object being processed</param>
    /// <param name="prefix">Current key prefix</param>
    /// <param name="visitedObjects">Set of visited objects for cycle detection</param>
    private static void ToDictionaryInternal(Dictionary<string, string> dictionary, object? obj, string prefix,
        HashSet<object> visitedObjects)
    {
        if (obj == null) return;

        // Check for circular reference
        if (!visitedObjects.Add(obj))
        {
            throw new InvalidOperationException(
                $"Circular reference detected while serializing object of type {obj.GetType().Name}");
        }

        try
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
                    GenericToDictionary(dictionary, prefix, value, key, visitedObjects);
                }
                else
                {
                    ToDictionaryInternal(dictionary, value, key, visitedObjects);
                }
            }
        }
        finally
        {
            // Remove object from visited set when exiting recursion
            visitedObjects.Remove(obj);
        }
    }

    /// <summary>
    /// Handles conversion of generic types (Dictionary and List) to dictionary entries
    /// </summary>
    /// <param name="dictionary">Target dictionary to populate</param>
    /// <param name="prefix">Current key prefix</param>
    /// <param name="value">Generic object value to process</param>
    /// <param name="key">Current key for the value</param>
    /// <param name="visitedObjects">Set of visited objects for cycle detection</param>
    private static void GenericToDictionary(Dictionary<string, string> dictionary, string prefix, object value,
        string key, HashSet<object> visitedObjects)
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
                        ToDictionaryInternal(dictionary, keyValue.Value!, key2, visitedObjects);
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
                    ToDictionaryInternal(dictionary, item, $"{key}:{i}", visitedObjects);
                }

                i++;
            }
        }
    }

    /// <summary>
    /// Converts object to string using invariant culture for formattable types
    /// </summary>
    /// <param name="obj">Object to convert to string</param>
    /// <returns>String representation of the object</returns>
    private static string ToStringInvariant(object? obj)
    {
        return obj switch
        {
            null => string.Empty,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => obj.ToString()!
        };
    }

    /// <summary>
    /// Determines if the object is a simple type or null
    /// </summary>
    /// <param name="obj">Object to check</param>
    /// <returns>True if object is simple type or null, otherwise false</returns>
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