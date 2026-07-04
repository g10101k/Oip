using System.Text.Json;
using Oip.Data.Dtos;
using Oip.Data.Entities;
using Oip.Users.Base.Contexts;

namespace Oip.Users.Base.Services;

internal static class UserExtensionMetadataMapper
{
    public const string EntityCode = "User";
    public const string TableName = "UserExtension";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static ExtensionFieldMetadataDto ToDto(ExtensionFieldMetadataEntity entity)
    {
        return new ExtensionFieldMetadataDto
        {
            Id = entity.Id,
            EntityCode = entity.EntityCode,
            TableSchema = entity.TableSchema,
            TableName = entity.TableName,
            FieldName = entity.FieldName,
            DbColumn = entity.DbColumn,
            Type = entity.Type,
            Options = string.IsNullOrWhiteSpace(entity.OptionsJson)
                ? []
                : JsonSerializer.Deserialize<List<ExtensionFieldOptionDto>>(entity.OptionsJson, JsonOptions) ?? [],
            IsRequired = entity.IsRequired,
            IsVisible = entity.IsVisible,
            IsSortable = entity.IsSortable,
            IsFilterable = entity.IsFilterable,
            Order = entity.Order
        };
    }

    public static ExtensionFieldMetadataEntity ToEntity(
        string fieldName,
        string dbColumn,
        ExtensionFieldType type,
        List<ExtensionFieldOptionDto> options,
        bool isRequired,
        bool isVisible,
        bool isSortable,
        bool isFilterable,
        int order)
    {
        return new ExtensionFieldMetadataEntity
        {
            EntityCode = EntityCode,
            TableSchema = UserContext.SchemaName,
            TableName = TableName,
            FieldName = fieldName,
            DbColumn = dbColumn,
            Type = type,
            OptionsJson = JsonSerializer.Serialize(options, JsonOptions),
            IsRequired = isRequired,
            IsVisible = isVisible,
            IsSortable = isSortable,
            IsFilterable = isFilterable,
            Order = order
        };
    }

    public static void ApplyMetadata(
        ExtensionFieldMetadataEntity entity,
        List<ExtensionFieldOptionDto> options,
        bool isRequired,
        bool isVisible,
        bool isSortable,
        bool isFilterable,
        int order)
    {
        entity.OptionsJson = JsonSerializer.Serialize(options, JsonOptions);
        entity.IsRequired = isRequired;
        entity.IsVisible = isVisible;
        entity.IsSortable = isSortable;
        entity.IsFilterable = isFilterable;
        entity.Order = order;
    }
}
