using Microsoft.EntityFrameworkCore;
using Oip.Data.Dtos;
using Oip.Users.Base.Contexts;

namespace Oip.Users.Base.Services;

/// <summary>
/// Executes provider-specific DDL for physical user extension columns.
/// </summary>
public class UserExtensionDdlService(UserContext context)
{
    /// <summary>
    /// Adds a whitelisted extension column.
    /// </summary>
    public async Task AddColumnAsync(string columnName, ExtensionFieldType type, CancellationToken cancellationToken)
    {
        var sqlType = GetSqlType(type);
        var sql = $"alter table {Quote(UserContext.SchemaName)}.{Quote(UserExtensionMetadataMapper.TableName)} " +
                  $"add {Quote(columnName)} {sqlType} null";

        await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    /// <summary>
    /// Drops an extension column.
    /// </summary>
    public async Task DropColumnAsync(string columnName, CancellationToken cancellationToken)
    {
        var sql = $"alter table {Quote(UserContext.SchemaName)}.{Quote(UserExtensionMetadataMapper.TableName)} " +
                  $"drop column {Quote(columnName)}";

        await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    private string GetSqlType(ExtensionFieldType type)
    {
        if (context.Database.IsNpgsql())
        {
            return type switch
            {
                ExtensionFieldType.Text => "text",
                ExtensionFieldType.Number => "numeric(18,4)",
                ExtensionFieldType.Boolean => "boolean",
                ExtensionFieldType.Date => "date",
                ExtensionFieldType.DateTime => "timestamp with time zone",
                ExtensionFieldType.Select => "text",
                _ => throw new NotSupportedException($"Unsupported extension type {type}.")
            };
        }

        if (context.Database.IsSqlServer())
        {
            return type switch
            {
                ExtensionFieldType.Text => "nvarchar(max)",
                ExtensionFieldType.Number => "decimal(18,4)",
                ExtensionFieldType.Boolean => "bit",
                ExtensionFieldType.Date => "date",
                ExtensionFieldType.DateTime => "datetimeoffset",
                ExtensionFieldType.Select => "nvarchar(max)",
                _ => throw new NotSupportedException($"Unsupported extension type {type}.")
            };
        }

        throw new NotSupportedException("Dynamic extension DDL is supported only for PostgreSQL and SQL Server.");
    }

    private string Quote(string identifier)
    {
        ExtensionFieldValidator.NormalizeIdentifier(identifier, "identifier");
        return context.Database.IsSqlServer()
            ? $"[{identifier}]"
            : $"\"{identifier}\"";
    }
}
