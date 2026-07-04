using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Oip.Base.Exceptions;
using Oip.Data.Dtos;
using Oip.Users.Base.Contexts;
using Oip.Users.Base.Dtos;

namespace Oip.Users.Base.Services;

/// <summary>
/// Manages global user extension field metadata and matching physical columns.
/// </summary>
public class UserExtensionMetadataService(
    UserContext context,
    ExtensionFieldValidator validator,
    UserExtensionDdlService ddlService)
{
    /// <summary>
    /// Gets user extension fields.
    /// </summary>
    public async Task<List<ExtensionFieldMetadataDto>> GetFieldsAsync(CancellationToken cancellationToken)
    {
        var fields = await context.ExtensionFieldMetadata
            .AsNoTracking()
            .Where(x => x.EntityCode == UserExtensionMetadataMapper.EntityCode)
            .OrderBy(x => x.Order)
            .ThenBy(x => x.FieldName)
            .ToListAsync(cancellationToken);

        return fields.Select(UserExtensionMetadataMapper.ToDto).ToList();
    }

    /// <summary>
    /// Creates metadata and a physical column.
    /// </summary>
    public async Task<ExtensionFieldMetadataDto> CreateFieldAsync(
        CreateUserExtensionFieldRequest request,
        CancellationToken cancellationToken)
    {
        var (fieldName, dbColumn) = await validator.ValidateCreateAsync(
            request.FieldName,
            request.DbColumn,
            request.Type,
            cancellationToken);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        await ddlService.AddColumnAsync(dbColumn, request.Type, cancellationToken);

        var entity = UserExtensionMetadataMapper.ToEntity(
            fieldName,
            dbColumn,
            request.Type,
            request.Options,
            request.IsRequired,
            request.IsVisible,
            request.IsSortable,
            request.IsFilterable,
            request.Order);

        context.ExtensionFieldMetadata.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return UserExtensionMetadataMapper.ToDto(entity);
    }

    /// <summary>
    /// Updates metadata that does not require changing physical DDL.
    /// </summary>
    public async Task<ExtensionFieldMetadataDto> UpdateFieldAsync(
        int id,
        UpdateUserExtensionFieldRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await context.ExtensionFieldMetadata
            .FirstOrDefaultAsync(x => x.Id == id && x.EntityCode == UserExtensionMetadataMapper.EntityCode,
                cancellationToken);

        if (entity is null)
        {
            throw new ApiException("Extension field not found", $"Extension field with id {id} was not found.",
                StatusCodes.Status404NotFound);
        }

        if (!string.Equals(entity.FieldName, request.FieldName, StringComparison.Ordinal) ||
            !string.Equals(entity.DbColumn, request.DbColumn ?? request.FieldName, StringComparison.Ordinal) ||
            entity.Type != request.Type)
        {
            throw new ApiException("Invalid extension field",
                "Changing field name, database column, or type requires a dedicated DDL operation.",
                StatusCodes.Status400BadRequest);
        }

        UserExtensionMetadataMapper.ApplyMetadata(
            entity,
            request.Options,
            request.IsRequired,
            request.IsVisible,
            request.IsSortable,
            request.IsFilterable,
            request.Order);

        await context.SaveChangesAsync(cancellationToken);
        return UserExtensionMetadataMapper.ToDto(entity);
    }

    /// <summary>
    /// Deletes metadata and drops the physical column.
    /// </summary>
    public async Task DeleteFieldAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await context.ExtensionFieldMetadata
            .FirstOrDefaultAsync(x => x.Id == id && x.EntityCode == UserExtensionMetadataMapper.EntityCode,
                cancellationToken);

        if (entity is null)
        {
            throw new ApiException("Extension field not found", $"Extension field with id {id} was not found.",
                StatusCodes.Status404NotFound);
        }

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        await ddlService.DropColumnAsync(entity.DbColumn, cancellationToken);
        context.ExtensionFieldMetadata.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
