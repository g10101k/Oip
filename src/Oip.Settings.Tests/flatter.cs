public static class ObjectFlattener
{
    public static IDictionary<string, string?> Flatten(object obj)
    {
        var result = new Dictionary<string, string?>();
        FlattenInternal(obj, result, parentPath: null);
        return result;
    }

    private static void FlattenInternal(object? obj, IDictionary<string, string?> dict, string? parentPath)
    {
        if (obj == null)
            return;

        var type = obj.GetType();

        if (IsSimple(type))
        {
            dict[parentPath!] = obj.ToString();
            return;
        }

        foreach (var prop in type.GetProperties())
        {
            if (!prop.CanRead) continue;

            var value = prop.GetValue(obj);
            var path = parentPath == null ? prop.Name : $"{parentPath}:{prop.Name}";

            if (value == null)
            {
                dict[path] = null;
                continue;
            }

            var valueType = value.GetType();

            if (IsSimple(valueType))
            {
                dict[path] = value.ToString();
            }
            else
            {
                FlattenInternal(value, dict, path);
            }
        }
    }

    private static bool IsSimple(Type type)
    {
        return type.IsPrimitive ||
               type.IsEnum ||
               type == typeof(string) ||
               type == typeof(decimal) ||
               type == typeof(DateTime) ||
               type == typeof(Guid) ||
               type == typeof(TimeSpan);
    }
}