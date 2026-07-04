using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Oip.Base.Exceptions;
using Oip.Data.Dtos;
using Oip.Users.Base.Contexts;
using Oip.Users.Base.Dtos;

namespace Oip.Users.Base.Services;

/// <summary>
/// Provides an extension-aware user table API backed by physical extension columns.
/// </summary>
public class UserExtensionTableService(UserContext context)
{
    private static readonly Dictionary<string, (string Column, ExtensionFieldType Type)> BaseColumns =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["userId"] = ("UserId", ExtensionFieldType.Number),
            ["email"] = ("Email", ExtensionFieldType.Text),
            ["firstName"] = ("FirstName", ExtensionFieldType.Text),
            ["lastName"] = ("LastName", ExtensionFieldType.Text),
            ["isActive"] = ("IsActive", ExtensionFieldType.Boolean),
            ["createdAt"] = ("CreatedAt", ExtensionFieldType.DateTime)
        };

    /// <summary>
    /// Gets a page of users and dynamic extension values.
    /// </summary>
    public async Task<ExtensionTablePageResult<UserExtensionTableRowDto>> GetPageAsync(
        TableQueryRequest request,
        CancellationToken cancellationToken)
    {
        var fields = await LoadFieldsAsync(cancellationToken);
        var columns = BuildColumns(fields);
        var normalizedFirst = Math.Max(0, request.First);
        var normalizedRows = request.Rows <= 0 ? 25 : Math.Min(request.Rows, 1000);
        var query = BuildQuery(request, fields);

        var total = await ExecuteScalarAsync<int>(
            $"select count(*) from {Quote(UserContext.SchemaName)}.{Quote("User")} u " +
            $"left join {Quote(UserContext.SchemaName)}.{Quote(UserExtensionMetadataMapper.TableName)} ext " +
            $"on ext.{Quote("UserId")} = u.{Quote("UserId")} {query.WhereSql}",
            query.Parameters,
            cancellationToken);

        var selectExtensionColumns = fields.Count == 0
            ? string.Empty
            : ", " + string.Join(", ", fields.Select(x => $"ext.{Quote(x.DbColumn)} as {Quote(x.FieldName)}"));

        var sql = $"select u.{Quote("UserId")} as {Quote("UserId")}, " +
                  $"u.{Quote("Email")} as {Quote("Email")}, " +
                  $"u.{Quote("FirstName")} as {Quote("FirstName")}, " +
                  $"u.{Quote("LastName")} as {Quote("LastName")}, " +
                  $"u.{Quote("IsActive")} as {Quote("IsActive")}, " +
                  $"u.{Quote("CreatedAt")} as {Quote("CreatedAt")}" +
                  selectExtensionColumns +
                  $" from {Quote(UserContext.SchemaName)}.{Quote("User")} u " +
                  $"left join {Quote(UserContext.SchemaName)}.{Quote(UserExtensionMetadataMapper.TableName)} ext " +
                  $"on ext.{Quote("UserId")} = u.{Quote("UserId")} {query.WhereSql} {query.OrderBySql} " +
                  OffsetSql(normalizedFirst, normalizedRows);

        query.Parameters["offset"] = normalizedFirst;
        query.Parameters["rows"] = normalizedRows;
        var data = await ExecuteRowsAsync(sql, query.Parameters, fields, cancellationToken);

        return new ExtensionTablePageResult<UserExtensionTableRowDto>(
            data,
            total,
            normalizedFirst,
            normalizedRows,
            columns);
    }

    /// <summary>
    /// Updates extension values for a user.
    /// </summary>
    public async Task UpdateValuesAsync(
        int userId,
        Dictionary<string, object?> values,
        CancellationToken cancellationToken)
    {
        var fields = await LoadFieldsAsync(cancellationToken);
        var fieldsByName = fields.ToDictionary(x => x.FieldName, StringComparer.OrdinalIgnoreCase);
        var assignments = new List<string>();
        var parameters = new Dictionary<string, object?> { ["userId"] = userId };
        var index = 0;

        foreach (var (fieldName, value) in values)
        {
            if (!fieldsByName.TryGetValue(fieldName, out var field))
            {
                throw new ApiException("Invalid extension values",
                    $"Extension field '{fieldName}' does not exist.",
                    StatusCodes.Status400BadRequest);
            }

            var parameterName = $"p{index++}";
            assignments.Add($"{Quote(field.DbColumn)} = {Parameter(parameterName)}");
            parameters[parameterName] = NormalizeValue(value, field.Type);
        }

        if (assignments.Count == 0)
        {
            return;
        }

        var sql = $"update {Quote(UserContext.SchemaName)}.{Quote(UserExtensionMetadataMapper.TableName)} " +
                  $"set {string.Join(", ", assignments)} where {Quote("UserId")} = {Parameter("userId")}";

        await ExecuteNonQueryAsync(sql, parameters, cancellationToken);
    }

    private async Task<List<ExtensionFieldMetadataDto>> LoadFieldsAsync(CancellationToken cancellationToken)
    {
        var entities = await context.ExtensionFieldMetadata
            .AsNoTracking()
            .Where(x => x.EntityCode == UserExtensionMetadataMapper.EntityCode)
            .OrderBy(x => x.Order)
            .ThenBy(x => x.FieldName)
            .ToListAsync(cancellationToken);

        return entities.Select(UserExtensionMetadataMapper.ToDto).ToList();
    }

    private static List<ExtensionTableColumnDto> BuildColumns(List<ExtensionFieldMetadataDto> fields)
    {
        var columns = new List<ExtensionTableColumnDto>
        {
            new() { Field = "email", Header = "Email", Type = ExtensionFieldType.Text, IsBase = true, Order = 10 },
            new() { Field = "firstName", Header = "First name", Type = ExtensionFieldType.Text, IsBase = true, Order = 20 },
            new() { Field = "lastName", Header = "Last name", Type = ExtensionFieldType.Text, IsBase = true, Order = 30 },
            new() { Field = "isActive", Header = "Active", Type = ExtensionFieldType.Boolean, IsBase = true, Order = 40 },
            new() { Field = "createdAt", Header = "Created", Type = ExtensionFieldType.DateTime, IsBase = true, Order = 50 }
        };

        columns.AddRange(fields.Select(field => new ExtensionTableColumnDto
        {
            Field = field.FieldName,
            Header = field.FieldName,
            Type = field.Type,
            IsBase = false,
            IsVisible = field.IsVisible,
            IsSortable = field.IsSortable,
            IsFilterable = field.IsFilterable,
            Order = 1000 + field.Order,
            Options = field.Options
        }));

        return columns.OrderBy(x => x.Order).ToList();
    }

    private QueryParts BuildQuery(TableQueryRequest request, List<ExtensionFieldMetadataDto> fields)
    {
        var parameters = new Dictionary<string, object?>();
        var where = new List<string>();
        var fieldByName = fields.ToDictionary(x => x.FieldName, StringComparer.OrdinalIgnoreCase);
        var parameterIndex = 0;

        foreach (var (fieldName, rawFilter) in request.Filters)
        {
            if (string.Equals(fieldName, "global", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var value = ExtractFilterValue(rawFilter);
            if (value is null)
            {
                continue;
            }

            var column = ResolveColumn(fieldName, fieldByName, requireFilterable: true);
            var parameterName = $"f{parameterIndex++}";
            where.Add(BuildPredicate(column, value, parameterName, parameters));
        }

        if (!string.IsNullOrWhiteSpace(request.GlobalFilter))
        {
            var globalClauses = new List<string>();
            foreach (var fieldName in new[] { "email", "firstName", "lastName" })
            {
                var parameterName = $"g{parameterIndex++}";
                parameters[parameterName] = $"%{request.GlobalFilter.Trim()}%";
                globalClauses.Add($"u.{Quote(BaseColumns[fieldName].Column)} like {Parameter(parameterName)}");
            }

            where.Add("(" + string.Join(" or ", globalClauses) + ")");
        }

        var orderColumn = ResolveColumnOrDefault(request.SortField, fieldByName);
        var orderDirection = request.SortOrder < 0 ? "desc" : "asc";
        return new QueryParts(
            where.Count == 0 ? string.Empty : "where " + string.Join(" and ", where),
            $"order by {orderColumn} {orderDirection}",
            parameters);
    }

    private string ResolveColumn(
        string fieldName,
        Dictionary<string, ExtensionFieldMetadataDto> extensionFields,
        bool requireFilterable = false)
    {
        if (BaseColumns.TryGetValue(fieldName, out var baseColumn))
        {
            return $"u.{Quote(baseColumn.Column)}";
        }

        if (extensionFields.TryGetValue(fieldName, out var extensionField))
        {
            if (requireFilterable && !extensionField.IsFilterable)
            {
                throw new ApiException("Invalid user extension table query",
                    $"Extension field '{fieldName}' is not filterable.",
                    StatusCodes.Status400BadRequest);
            }

            return $"ext.{Quote(extensionField.DbColumn)}";
        }

        throw new ApiException("Invalid user extension table query",
            $"Unknown table field '{fieldName}'.",
            StatusCodes.Status400BadRequest);
    }

    private string ResolveColumnOrDefault(
        string? fieldName,
        Dictionary<string, ExtensionFieldMetadataDto> extensionFields)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return $"u.{Quote("UserId")}";
        }

        if (extensionFields.TryGetValue(fieldName, out var extensionField) && !extensionField.IsSortable)
        {
            throw new ApiException("Invalid user extension table query",
                $"Extension field '{fieldName}' is not sortable.",
                StatusCodes.Status400BadRequest);
        }

        return ResolveColumn(fieldName, extensionFields);
    }

    private string BuildPredicate(
        string column,
        object value,
        string parameterName,
        Dictionary<string, object?> parameters)
    {
        if (value is string stringValue)
        {
            parameters[parameterName] = $"%{stringValue}%";
            return $"{column} like {Parameter(parameterName)}";
        }

        parameters[parameterName] = value;
        return $"{column} = {Parameter(parameterName)}";
    }

    private static object? ExtractFilterValue(JsonElement rawFilter)
    {
        if (rawFilter.ValueKind == JsonValueKind.Object)
        {
            if (rawFilter.TryGetProperty("value", out var valueElement))
            {
                return ReadJsonValue(valueElement);
            }

            if (rawFilter.TryGetProperty("constraints", out var constraintsElement) &&
                constraintsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var constraint in constraintsElement.EnumerateArray())
                {
                    if (constraint.TryGetProperty("value", out var constraintValue))
                    {
                        var value = ReadJsonValue(constraintValue);
                        if (value is not null)
                        {
                            return value;
                        }
                    }
                }
            }
        }

        return ReadJsonValue(rawFilter);
    }

    private static object? ReadJsonValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.TryGetInt64(out var longValue)
                ? longValue
                : value.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => null,
            _ => value.GetRawText()
        };
    }

    private static object? NormalizeValue(object? value, ExtensionFieldType type)
    {
        if (value is null)
        {
            return null;
        }

        if (value is JsonElement json)
        {
            value = ReadJsonValue(json);
        }

        return type switch
        {
            ExtensionFieldType.Number => Convert.ToDecimal(value),
            ExtensionFieldType.Boolean => Convert.ToBoolean(value),
            ExtensionFieldType.Date => DateOnly.TryParse(Convert.ToString(value), out var date) ? date : value,
            ExtensionFieldType.DateTime => DateTimeOffset.TryParse(Convert.ToString(value), out var dateTime)
                ? dateTime
                : value,
            _ => Convert.ToString(value)
        };
    }

    private async Task<List<UserExtensionTableRowDto>> ExecuteRowsAsync(
        string sql,
        Dictionary<string, object?> parameters,
        List<ExtensionFieldMetadataDto> fields,
        CancellationToken cancellationToken)
    {
        await using var command = await CreateCommandAsync(sql, parameters, cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var rows = new List<UserExtensionTableRowDto>();

        while (await reader.ReadAsync(cancellationToken))
        {
            var row = new UserExtensionTableRowDto
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("CreatedAt"))
            };

            foreach (var field in fields)
            {
                var ordinal = reader.GetOrdinal(field.FieldName);
                row.ExtensionValues[field.FieldName] = await reader.IsDBNullAsync(ordinal, cancellationToken)
                    ? null
                    : reader.GetValue(ordinal);
            }

            rows.Add(row);
        }

        return rows;
    }

    private async Task<T> ExecuteScalarAsync<T>(
        string sql,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken)
    {
        await using var command = await CreateCommandAsync(sql, parameters, cancellationToken);
        var value = await command.ExecuteScalarAsync(cancellationToken);
        return (T)Convert.ChangeType(value!, typeof(T));
    }

    private async Task ExecuteNonQueryAsync(
        string sql,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken)
    {
        await using var command = await CreateCommandAsync(sql, parameters, cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<System.Data.Common.DbCommand> CreateCommandAsync(
        string sql,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (var (name, value) in parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = Parameter(name);
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }

        return command;
    }

    private string OffsetSql(int first, int rows)
    {
        return context.Database.IsSqlServer()
            ? $"offset {Parameter("offset")} rows fetch next {Parameter("rows")} rows only"
            : $"offset {Parameter("offset")} limit {Parameter("rows")}";
    }

    private string Parameter(string name)
    {
        return context.Database.IsSqlServer() ? $"@{name}" : $"@{name}";
    }

    private string Quote(string identifier)
    {
        ExtensionFieldValidator.NormalizeIdentifier(identifier, "identifier");
        return context.Database.IsSqlServer()
            ? $"[{identifier}]"
            : $"\"{identifier}\"";
    }

    private sealed record QueryParts(
        string WhereSql,
        string OrderBySql,
        Dictionary<string, object?> Parameters);
}
