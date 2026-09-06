using System.Collections;
using System.Reflection;
using Oip.Base.Settings.Attributes;

namespace Oip.Base.Security.DefaultSecrets;

/// <summary>
/// Walks a settings object graph and collects every property marked with <see cref="SecretSettingAttribute"/>.
/// </summary>
public static class SecretSettingsScanner
{
    private const int MaxDepth = 10;

    /// <summary>
    /// Collects the secret bearing settings reachable from <paramref name="root"/>.
    /// </summary>
    /// <param name="root">Root settings object, normally the application <c>AppSettings</c> instance.</param>
    /// <returns>The discovered secret bearing settings.</returns>
    public static IReadOnlyList<SecretSettingDescriptor> Scan(object? root)
    {
        var result = new List<SecretSettingDescriptor>();
        if (root is null)
            return result;

        Visit(root, prefix: null, depth: 0, new HashSet<object>(ReferenceEqualityComparer.Instance), result);
        return result;
    }

    private static void Visit(object instance, string? prefix, int depth, HashSet<object> visited,
        List<SecretSettingDescriptor> result)
    {
        if (depth > MaxDepth || !visited.Add(instance))
            return;

        var properties = instance.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.CanRead && x.GetIndexParameters().Length == 0);

        foreach (var property in properties)
        {
            var attribute = property.GetCustomAttribute<SecretSettingAttribute>();
            var key = attribute?.ConfigKey ?? Combine(prefix, property.Name);

            object? value;
            try
            {
                value = property.GetValue(instance);
            }
            catch (TargetInvocationException)
            {
                continue;
            }

            if (attribute is not null)
            {
                result.Add(new SecretSettingDescriptor(key, value as string, attribute.InsecureValues,
                    attribute.Required, attribute.OverrideHint));
                continue;
            }

            if (value is not null && ShouldVisit(property.PropertyType))
                Visit(value, key, depth + 1, visited, result);
        }
    }

    private static bool ShouldVisit(Type type)
    {
        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal)
            || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(TimeSpan)
            || type == typeof(Guid) || type == typeof(Uri))
            return false;

        if (typeof(IEnumerable).IsAssignableFrom(type))
            return false;

        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying is not null)
            return ShouldVisit(underlying);

        // Only settings types of this platform are walked, framework types never carry the attribute.
        return type.Namespace?.StartsWith("Oip", StringComparison.Ordinal) == true;
    }

    private static string Combine(string? prefix, string name) =>
        string.IsNullOrEmpty(prefix) ? name : $"{prefix}:{name}";
}
