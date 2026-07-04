using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Oip.Base.Exceptions;
using Oip.Data.Dtos;
using Oip.Users.Base.Contexts;

namespace Oip.Users.Base.Services;

/// <summary>
/// Validates extension field metadata before dynamic DDL is generated.
/// </summary>
public partial class ExtensionFieldValidator(UserContext context)
{
    private static readonly HashSet<ExtensionFieldType> SupportedTypes =
    [
        ExtensionFieldType.Text,
        ExtensionFieldType.Number,
        ExtensionFieldType.Boolean,
        ExtensionFieldType.Date,
        ExtensionFieldType.DateTime,
        ExtensionFieldType.Select
    ];

    /// <summary>
    /// Validates a new extension field.
    /// </summary>
    public async Task<(string FieldName, string DbColumn)> ValidateCreateAsync(
        string fieldName,
        string? dbColumn,
        ExtensionFieldType type,
        CancellationToken cancellationToken)
    {
        var normalizedFieldName = NormalizeIdentifier(fieldName, "field name");
        var normalizedDbColumn = NormalizeIdentifier(dbColumn ?? fieldName, "database column");

        if (!SupportedTypes.Contains(type))
        {
            throw new ApiException("Invalid extension field", $"Unsupported extension field type '{type}'.",
                StatusCodes.Status400BadRequest);
        }

        var duplicate = await context.ExtensionFieldMetadata
            .AsNoTracking()
            .AnyAsync(x => x.EntityCode == UserExtensionMetadataMapper.EntityCode &&
                           (x.FieldName == normalizedFieldName || x.DbColumn == normalizedDbColumn),
                cancellationToken);

        if (duplicate)
        {
            throw new ApiException("Invalid extension field",
                "Extension field name or database column already exists.",
                StatusCodes.Status400BadRequest);
        }

        return (normalizedFieldName, normalizedDbColumn);
    }

    /// <summary>
    /// Validates an identifier.
    /// </summary>
    public static string NormalizeIdentifier(string value, string label)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ApiException("Invalid extension field", $"Extension {label} is required.",
                StatusCodes.Status400BadRequest);
        }

        var normalized = value.Trim();
        if (!IdentifierRegex().IsMatch(normalized))
        {
            throw new ApiException("Invalid extension field",
                $"Extension {label} must start with a letter and contain only letters, numbers, or underscores.",
                StatusCodes.Status400BadRequest);
        }

        return normalized;
    }

    [GeneratedRegex("^[A-Za-z][A-Za-z0-9_]{0,127}$")]
    private static partial Regex IdentifierRegex();
}
