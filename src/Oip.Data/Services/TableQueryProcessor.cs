using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Oip.Data.Dtos;

namespace Oip.Data.Services;

/// <summary>
/// Applies PrimeNG-style table filtering, sorting, and paging to an <see cref="IQueryable{T}"/>.
/// </summary>
public static class TableQueryProcessor
{
    /// <summary>
    /// Executes a server-side table query and projects the results to a DTO.
    /// </summary>
    public static async Task<TablePageResult<TProjection>> ExecuteAsync<TEntity, TProjection>(
        IQueryable<TEntity> query,
        TableQueryRequest request,
        Expression<Func<TEntity, TProjection>> selector,
        TableQueryOptions? options = null,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(selector);

        options ??= new TableQueryOptions();

        var normalizedRows = NormalizeRows(request.Rows, options);
        var normalizedFirst = request.First < 0 ? 0 : request.First;

        query = ApplyFilters(query, request, options);
        query = ApplySorting(query, request.SortField, request.SortOrder, options);

        var total = await query.CountAsync(cancellationToken);
        var data = await query
            .Skip(normalizedFirst)
            .Take(normalizedRows)
            .Select(selector)
            .ToListAsync(cancellationToken);

        return new TablePageResult<TProjection>(data, total, normalizedFirst, normalizedRows);
    }

    private static int NormalizeRows(int rows, TableQueryOptions options)
    {
        if (rows <= 0)
        {
            return options.DefaultRows;
        }

        if (rows > options.MaxRows)
        {
            throw new ArgumentException(
                $"Requested page size {rows} exceeds the configured limit of {options.MaxRows}.");
        }

        return rows;
    }

    private static IQueryable<TEntity> ApplyFilters<TEntity>(
        IQueryable<TEntity> query,
        TableQueryRequest request,
        TableQueryOptions options)
        where TEntity : class
    {
        if (request.Filters.Count > 0)
        {
            foreach (var filterPair in request.Filters)
            {
                if (string.Equals(filterPair.Key, "global", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var normalizedFilter = NormalizeFilter(filterPair.Value);
                var predicate = BuildColumnPredicate<TEntity>(filterPair.Key, normalizedFilter, options);
                if (predicate != null)
                {
                    query = query.Where(predicate);
                }
            }
        }

        var globalValue = request.GlobalFilter;
        if (string.IsNullOrWhiteSpace(globalValue) &&
            request.Filters.TryGetValue("global", out var globalFilter))
        {
            globalValue = ExtractFirstStringValue(NormalizeFilter(globalFilter));
        }

        if (!string.IsNullOrWhiteSpace(globalValue))
        {
            query = query.Where(BuildGlobalPredicate<TEntity>(globalValue!, options));
        }

        return query;
    }

    private static IQueryable<TEntity> ApplySorting<TEntity>(IQueryable<TEntity> query, string? sortField,
        int sortOrder, TableQueryOptions options)
    {
        if (string.IsNullOrWhiteSpace(sortField))
        {
            return query;
        }

        var propertyPath = options.ResolveField(sortField);
        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        var body = BuildPropertyExpression(parameter, propertyPath, out _);
        var lambda = Expression.Lambda(body, parameter);
        var methodName = sortOrder < 0 ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);

        var orderedExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(TEntity), body.Type },
            query.Expression,
            Expression.Quote(lambda));

        return query.Provider.CreateQuery<TEntity>(orderedExpression);
    }

    private static Expression<Func<TEntity, bool>>? BuildColumnPredicate<TEntity>(
        string fieldName,
        TableColumnFilter filter,
        TableQueryOptions options)
    {
        var constraints = GetConstraints(filter);
        if (constraints.Count == 0)
        {
            return null;
        }

        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        var propertyPath = options.ResolveField(fieldName);
        var propertyExpression = BuildPropertyExpression(parameter, propertyPath, out var nullGuard);

        Expression? combinedExpression = null;
        var useOr = string.Equals(filter.Operator, "or", StringComparison.OrdinalIgnoreCase);

        foreach (var constraint in constraints)
        {
            if (constraint.Value is null || IsNullOrEmpty(constraint.Value.Value))
            {
                continue;
            }

            var constraintExpression = BuildConstraintExpression(propertyExpression, nullGuard, constraint);
            combinedExpression = combinedExpression == null
                ? constraintExpression
                : useOr
                    ? Expression.OrElse(combinedExpression, constraintExpression)
                    : Expression.AndAlso(combinedExpression, constraintExpression);
        }

        return combinedExpression == null
            ? null
            : Expression.Lambda<Func<TEntity, bool>>(combinedExpression, parameter);
    }

    private static Expression<Func<TEntity, bool>> BuildGlobalPredicate<TEntity>(
        string globalValue,
        TableQueryOptions options)
    {
        if (options.GlobalFilterFields.Count == 0)
        {
            throw new ArgumentException("Global filter was requested, but no global filter fields are configured.");
        }

        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        Expression? combinedExpression = null;

        foreach (var configuredField in options.GlobalFilterFields)
        {
            var propertyPath = options.ResolveField(configuredField);
            var propertyExpression = BuildPropertyExpression(parameter, propertyPath, out var nullGuard);
            if (propertyExpression.Type != typeof(string))
            {
                throw new ArgumentException(
                    $"Global filter field '{configuredField}' must point to a string property.");
            }

            var containsExpression = BuildStringComparison(propertyExpression, "contains", globalValue);
            var safeExpression = nullGuard == null
                ? containsExpression
                : Expression.AndAlso(nullGuard, containsExpression);

            combinedExpression = combinedExpression == null
                ? safeExpression
                : Expression.OrElse(combinedExpression, safeExpression);
        }

        return Expression.Lambda<Func<TEntity, bool>>(combinedExpression!, parameter);
    }

    private static List<TableFilterConstraint> GetConstraints(TableColumnFilter filter)
    {
        if (filter.Constraints.Count > 0)
        {
            return filter.Constraints;
        }

        if (filter.Value is not null || !string.IsNullOrWhiteSpace(filter.MatchMode))
        {
            return
            [
                new TableFilterConstraint
                {
                    Value = filter.Value,
                    MatchMode = filter.MatchMode
                }
            ];
        }

        return new List<TableFilterConstraint>();
    }

    private static TableColumnFilter NormalizeFilter(JsonElement rawFilter)
    {
        return rawFilter.ValueKind switch
        {
            JsonValueKind.Object => NormalizeFilterObject(rawFilter),
            JsonValueKind.Array => new TableColumnFilter
            {
                Constraints = rawFilter.EnumerateArray()
                    .Select(NormalizeConstraint)
                    .ToList()
            },
            _ => throw new ArgumentException("Unsupported filter payload format.")
        };
    }

    private static TableColumnFilter NormalizeFilterObject(JsonElement rawFilter)
    {
        var filter = new TableColumnFilter();

        if (rawFilter.TryGetProperty("operator", out var operatorElement) &&
            operatorElement.ValueKind == JsonValueKind.String)
        {
            filter.Operator = operatorElement.GetString();
        }

        if (rawFilter.TryGetProperty("value", out var valueElement))
        {
            filter.Value = valueElement;
        }

        if (rawFilter.TryGetProperty("matchMode", out var matchModeElement) &&
            matchModeElement.ValueKind == JsonValueKind.String)
        {
            filter.MatchMode = matchModeElement.GetString();
        }

        if (rawFilter.TryGetProperty("constraints", out var constraintsElement) &&
            constraintsElement.ValueKind == JsonValueKind.Array)
        {
            filter.Constraints = constraintsElement.EnumerateArray()
                .Select(NormalizeConstraint)
                .ToList();
        }

        return filter;
    }

    private static TableFilterConstraint NormalizeConstraint(JsonElement rawConstraint)
    {
        if (rawConstraint.ValueKind != JsonValueKind.Object)
        {
            return new TableFilterConstraint
            {
                Value = rawConstraint
            };
        }

        var constraint = new TableFilterConstraint();

        if (rawConstraint.TryGetProperty("value", out var valueElement))
        {
            constraint.Value = valueElement;
        }

        if (rawConstraint.TryGetProperty("matchMode", out var matchModeElement) &&
            matchModeElement.ValueKind == JsonValueKind.String)
        {
            constraint.MatchMode = matchModeElement.GetString();
        }

        return constraint;
    }

    private static Expression BuildConstraintExpression(
        Expression propertyExpression,
        Expression? nullGuard,
        TableFilterConstraint constraint)
    {
        var matchMode = string.IsNullOrWhiteSpace(constraint.MatchMode)
            ? "equals"
            : constraint.MatchMode!;

        Expression comparisonExpression;
        if (propertyExpression.Type == typeof(string))
        {
            var stringValue = ExtractString(constraint.Value!.Value);
            comparisonExpression = BuildStringComparison(propertyExpression, matchMode, stringValue);
        }
        else
        {
            comparisonExpression = BuildTypedComparison(propertyExpression, matchMode, constraint.Value!.Value);
        }

        return nullGuard == null
            ? comparisonExpression
            : Expression.AndAlso(nullGuard, comparisonExpression);
    }

    private static Expression BuildStringComparison(Expression propertyExpression, string matchMode, string value)
    {
        var notNullExpression = Expression.NotEqual(propertyExpression, Expression.Constant(null, typeof(string)));
        var normalizedPropertyExpression = Expression.Call(propertyExpression, nameof(string.ToLower), Type.EmptyTypes);
        var normalizedValueExpression = Expression.Constant(value.ToLowerInvariant());

        Expression comparisonExpression = matchMode.ToLowerInvariant() switch
        {
            "contains" => Expression.Call(normalizedPropertyExpression, nameof(string.Contains), Type.EmptyTypes,
                normalizedValueExpression),
            "startswith" => Expression.Call(normalizedPropertyExpression, nameof(string.StartsWith), Type.EmptyTypes,
                normalizedValueExpression),
            "endswith" => Expression.Call(normalizedPropertyExpression, nameof(string.EndsWith), Type.EmptyTypes,
                normalizedValueExpression),
            "equals" => Expression.Equal(normalizedPropertyExpression, normalizedValueExpression),
            "notequals" => Expression.NotEqual(normalizedPropertyExpression, normalizedValueExpression),
            _ => throw new ArgumentException($"Match mode '{matchMode}' is not supported for string properties.")
        };

        return matchMode.Equals("notequals", StringComparison.OrdinalIgnoreCase)
            ? Expression.OrElse(Expression.Equal(propertyExpression, Expression.Constant(null, typeof(string))),
                comparisonExpression)
            : Expression.AndAlso(notNullExpression, comparisonExpression);
    }

    private static Expression BuildTypedComparison(Expression propertyExpression, string matchMode, JsonElement value)
    {
        if (string.Equals(matchMode, "in", StringComparison.OrdinalIgnoreCase))
        {
            return BuildInExpression(propertyExpression, value);
        }

        if (IsDateComparison(propertyExpression.Type))
        {
            return BuildDateComparison(propertyExpression, matchMode, value);
        }

        var targetType = Nullable.GetUnderlyingType(propertyExpression.Type) ?? propertyExpression.Type;
        var convertedValue = ConvertJsonElement(value, targetType);
        var constant = Expression.Constant(convertedValue, targetType);
        var left = propertyExpression.Type == targetType
            ? propertyExpression
            : Expression.Convert(propertyExpression, targetType);

        return matchMode.ToLowerInvariant() switch
        {
            "equals" => Expression.Equal(left, constant),
            "notequals" => Expression.NotEqual(left, constant),
            "gt" or "greaterthan" or "after" or "dateafter" => Expression.GreaterThan(left, constant),
            "gte" or "greaterthanorequal" => Expression.GreaterThanOrEqual(left, constant),
            "lt" or "lessthan" or "before" or "datebefore" => Expression.LessThan(left, constant),
            "lte" or "lessthanorequal" => Expression.LessThanOrEqual(left, constant),
            _ => throw new ArgumentException(
                $"Match mode '{matchMode}' is not supported for property type '{targetType.Name}'.")
        };
    }

    private static bool IsDateComparison(Type propertyType)
    {
        var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        return targetType == typeof(DateTime) || targetType == typeof(DateTimeOffset);
    }

    private static Expression BuildDateComparison(Expression propertyExpression, string matchMode, JsonElement value)
    {
        var targetType = Nullable.GetUnderlyingType(propertyExpression.Type) ?? propertyExpression.Type;
        var convertedValue = ConvertJsonElement(value, targetType);
        var constant = Expression.Constant(convertedValue, targetType);
        var left = propertyExpression.Type == targetType
            ? propertyExpression
            : Expression.Convert(propertyExpression, targetType);

        if (targetType == typeof(DateTime))
        {
            var leftDate = Expression.Property(left, nameof(DateTime.Date));
            var rightDate = Expression.Property(constant, nameof(DateTime.Date));

            return matchMode.ToLowerInvariant() switch
            {
                "dateis" => Expression.Equal(leftDate, rightDate),
                "dateisnot" => Expression.NotEqual(leftDate, rightDate),
                "datebefore" => Expression.LessThan(leftDate, rightDate),
                "dateafter" => Expression.GreaterThan(leftDate, rightDate),
                "equals" => Expression.Equal(left, constant),
                "notequals" => Expression.NotEqual(left, constant),
                "gt" or "greaterthan" or "after" => Expression.GreaterThan(left, constant),
                "gte" or "greaterthanorequal" => Expression.GreaterThanOrEqual(left, constant),
                "lt" or "lessthan" or "before" => Expression.LessThan(left, constant),
                "lte" or "lessthanorequal" => Expression.LessThanOrEqual(left, constant),
                _ => throw new ArgumentException(
                    $"Match mode '{matchMode}' is not supported for property type 'DateTime'.")
            };
        }

        if (targetType == typeof(DateTimeOffset))
        {
            var leftUtcDateTime = Expression.Property(left, nameof(DateTimeOffset.UtcDateTime));
            var leftDate = Expression.Property(leftUtcDateTime, nameof(DateTime.Date));
            var rightUtcDateTime = Expression.Property(constant, nameof(DateTimeOffset.UtcDateTime));
            var rightDate = Expression.Property(rightUtcDateTime, nameof(DateTime.Date));

            return matchMode.ToLowerInvariant() switch
            {
                "dateis" => Expression.Equal(leftDate, rightDate),
                "dateisnot" => Expression.NotEqual(leftDate, rightDate),
                "datebefore" => Expression.LessThan(leftDate, rightDate),
                "dateafter" => Expression.GreaterThan(leftDate, rightDate),
                "equals" => Expression.Equal(left, constant),
                "notequals" => Expression.NotEqual(left, constant),
                "gt" or "greaterthan" or "after" => Expression.GreaterThan(left, constant),
                "gte" or "greaterthanorequal" => Expression.GreaterThanOrEqual(left, constant),
                "lt" or "lessthan" or "before" => Expression.LessThan(left, constant),
                "lte" or "lessthanorequal" => Expression.LessThanOrEqual(left, constant),
                _ => throw new ArgumentException(
                    $"Match mode '{matchMode}' is not supported for property type 'DateTimeOffset'.")
            };
        }

        throw new ArgumentException(
            $"Match mode '{matchMode}' is not supported for property type '{targetType.Name}'.");
    }

    private static Expression BuildInExpression(Expression propertyExpression, JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw new ArgumentException("The 'in' match mode expects an array value.");
        }

        var targetType = Nullable.GetUnderlyingType(propertyExpression.Type) ?? propertyExpression.Type;
        var values = value.EnumerateArray()
            .Select(item => ConvertJsonElement(item, targetType))
            .ToArray();

        var containsMethod = typeof(Enumerable).GetMethods()
            .Single(method => method.Name == nameof(Enumerable.Contains) &&
                              method.GetParameters().Length == 2)
            .MakeGenericMethod(targetType);

        var arrayExpression = Expression.Constant(values, targetType.MakeArrayType());
        var memberExpression = propertyExpression.Type == targetType
            ? propertyExpression
            : Expression.Convert(propertyExpression, targetType);

        return Expression.Call(containsMethod, arrayExpression, memberExpression);
    }

    private static Expression BuildPropertyExpression(
        Expression parameter,
        string propertyPath,
        out Expression? nullGuard)
    {
        Expression currentExpression = parameter;
        Expression? guardExpression = null;

        foreach (var segment in propertyPath.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (currentExpression != parameter && CanBeNull(currentExpression.Type))
            {
                var currentGuard = Expression.NotEqual(
                    currentExpression,
                    Expression.Constant(null, currentExpression.Type));
                guardExpression = guardExpression == null
                    ? currentGuard
                    : Expression.AndAlso(guardExpression, currentGuard);
            }

            currentExpression = Expression.PropertyOrField(currentExpression, segment);
        }

        nullGuard = guardExpression;
        return currentExpression;
    }

    private static bool CanBeNull(Type type)
    {
        return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
    }

    private static string ExtractFirstStringValue(TableColumnFilter filter)
    {
        var constraint = GetConstraints(filter).FirstOrDefault();
        if (constraint?.Value is not null)
        {
            return ExtractString(constraint.Value.Value);
        }

        return string.Empty;
    }

    private static string ExtractString(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => bool.TrueString,
            JsonValueKind.False => bool.FalseString,
            _ => value.ToString() ?? string.Empty
        };
    }

    private static bool IsNullOrEmpty(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Null => true,
            JsonValueKind.Undefined => true,
            JsonValueKind.String => string.IsNullOrWhiteSpace(value.GetString()),
            JsonValueKind.Array => value.GetArrayLength() == 0,
            _ => false
        };
    }

    private static object ConvertJsonElement(JsonElement value, Type targetType)
    {
        if (targetType == typeof(string))
        {
            return ExtractString(value);
        }

        if (targetType == typeof(int))
        {
            return value.GetInt32();
        }

        if (targetType == typeof(long))
        {
            return value.GetInt64();
        }

        if (targetType == typeof(decimal))
        {
            return value.GetDecimal();
        }

        if (targetType == typeof(double))
        {
            return value.GetDouble();
        }

        if (targetType == typeof(float))
        {
            return value.GetSingle();
        }

        if (targetType == typeof(bool))
        {
            return value.GetBoolean();
        }

        if (targetType == typeof(DateTime))
        {
            return value.ValueKind == JsonValueKind.String
                ? DateTime.Parse(value.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
                : value.GetDateTime();
        }

        if (targetType == typeof(DateTimeOffset))
        {
            return value.ValueKind == JsonValueKind.String
                ? DateTimeOffset.Parse(value.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
                : value.GetDateTimeOffset();
        }

        if (targetType.IsEnum)
        {
            if (value.ValueKind == JsonValueKind.String)
            {
                return Enum.Parse(targetType, value.GetString()!, true);
            }

            return Enum.ToObject(targetType, value.GetInt32());
        }

        throw new ArgumentException($"Unsupported filter value type '{targetType.Name}'.");
    }
}
