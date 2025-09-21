using Microsoft.EntityFrameworkCore;
using Oip.Base.Services;
using Oip.Rtds.Data.Contexts;
using Oip.Rtds.Data.Dtos;
using Oip.Rtds.Data.Entities;
using Oip.Rtds.Data.Enums;

namespace Oip.Rtds.Data.Repositories;

/// <summary>
/// Repository for managing tags and their associated data in RTDS (Real-Time Data System)
/// </summary>
/// <remarks>
/// This repository handles both metadata (in RtdsMetaContext) and time-series data (in RtdsContext)
/// </remarks>
public class TagRepository
{
    private readonly RtdsMetaContext _rtdsMetaContext;
    private readonly RtdsContext _rtdsContext;
    private readonly UserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TagRepository"/> class
    /// </summary>
    /// <param name="rtdsMetaContext">The context for tag metadata operations</param>
    /// <param name="rtdsContext">The context for time-series data operations</param>
    /// <param name="userService">The user service</param>
    public TagRepository(RtdsMetaContext rtdsMetaContext, RtdsContext rtdsContext, UserService userService)
    {
        _rtdsMetaContext = rtdsMetaContext;
        _rtdsContext = rtdsContext;
        _userService = userService;
    }

    /// <summary>
    /// Creates a new tag with the specified configuration
    /// </summary>
    /// <param name="createTag">The tag creation DTO containing all configuration parameters</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the specified tag value type is not supported
    /// </exception>
    public async Task AddTag(CreateTagDto createTag)
    {
        await _rtdsMetaContext.Database.BeginTransactionAsync();
        try
        {
            var tagEntity = await _rtdsMetaContext.Tags.AddAsync(new TagEntity
            {
                Name = createTag.Name,
                ValueType = createTag.ValueType,
                InterfaceId = createTag.Interface,
                Descriptor = createTag.Descriptor,
                Uom = createTag.Uom,
                InstrumentTag = createTag.InstrumentTag,
                Enabled = createTag.Enabled,
                Compressing = createTag.Compressing,
                CompressionMinTime = createTag.CompressionMinTime,
                CompressionMaxTime = createTag.CompressionMaxTime,
                DigitalSet = createTag.DigitalSet,
                Step = createTag.Step,
                CreationDate = DateTimeOffset.UtcNow,
                Creator = _userService.GetUserEmail() ?? throw new InvalidOperationException("User not found"),
                ValueCalculation = createTag.ValueCalculation,
                TimeCalculation = createTag.TimeCalculation,
                ErrorCalculation = createTag.ErrorCalculation
            });

            await _rtdsMetaContext.SaveChangesAsync();
            var tableName = $"{tagEntity.Entity.Id:D6}";
            var valueType = GetClickHouseTypeFromTagType(createTag.ValueType);
            var statusType = GenerateClickHouseEnum8<TagValueStatus>();

            await _rtdsContext.CreateTagTableAsync(valueType, statusType);
            await _rtdsMetaContext.Database.CommitTransactionAsync();
        }
        catch (Exception)
        {
            await _rtdsMetaContext.Database.RollbackTransactionAsync();
            throw;
        }
    }

    /// <summary>
    /// Retrieves a list of <see cref="TagEntity"/> objects from the database that match the specified filter.
    /// </summary>
    /// <param name="filter">A string filter to apply to tag names (e.g., "Sensor%").</param>
    /// <returns>A list of <see cref="TagEntity"/> objects matching the filter.</returns>
    public List<TagDto> GetTagsByFilter(string filter)
    {
        return _rtdsMetaContext.Tags
            .Where(tag => EF.Functions.Like(tag.Name, filter))
            .Select(x => new TagDto()
            {
                Id = x.Id,
                Name = x.Name,
                ValueType = x.ValueType,
                InterfaceId = x.InterfaceId,
                Descriptor = x.Descriptor,
                Uom = x.Uom,
                InstrumentTag = x.InstrumentTag,
                Enabled = x.Enabled,
                Compressing = x.Compressing,
                CompressionMinTime = x.CompressionMinTime,
                CompressionMaxTime = x.CompressionMaxTime,
                DigitalSet = x.DigitalSet,
                Step = x.Step,
                CreationDate = x.CreationDate,
                Creator = x.Creator,
                ValueCalculation = x.ValueCalculation,
                TimeCalculation = x.TimeCalculation,
                ErrorCalculation = x.ErrorCalculation,
            })
            .ToList();
    }

    /// <summary>
    /// Converts a <see cref="TagTypes"/> enum value to the corresponding ClickHouse data type
    /// </summary>
    /// <param name="pointType">The tag type to convert</param>
    /// <returns>The ClickHouse type name as string</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the specified tag type is not supported
    /// </exception>
    private static string GetClickHouseTypeFromTagType(TagTypes pointType)
    {
        return pointType switch
        {
            TagTypes.Float32 => "Float32",
            TagTypes.Float64 => "Float64",
            TagTypes.Int16 => "Int32",
            TagTypes.Int32 => "Int32",
            TagTypes.Digital => "UInt8",
            TagTypes.String => "String",
            TagTypes.Blob => "String",
            _ => throw new NotSupportedException($"Unsupported TagType: {pointType}")
        };
    }

    /// <summary>
    /// Generates a ClickHouse Enum8 type definition from a .NET enum type
    /// </summary>
    /// <typeparam name="TEnum">The enum type to convert</typeparam>
    /// <returns>ClickHouse Enum8 definition string</returns>
    private static string GenerateClickHouseEnum8<TEnum>() where TEnum : Enum
    {
        var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
        var entries = values.Select(v => $"'{v}' = {(int)(object)v}");
        return "Enum8(" + string.Join(", ", entries) + ")";
    }

    /// <summary>
    /// Edits an existing tag in the database.
    /// </summary>
    /// <param name="dto">The tag object containing the updated information.</param>
    /// <exception cref="InvalidOperationException">Thrown if the tag is not found.</exception>
    public void EditTag(CreateTagDto dto)
    {
        var entity = _rtdsMetaContext.Tags.FirstOrDefault(x => x.Id == dto.TagId) ??
                     throw new InvalidOperationException("Tag not found");
        entity.Name = dto.Name;
        entity.ValueType = dto.ValueType;
        entity.InterfaceId = dto.Interface;
        entity.Descriptor = dto.Descriptor;
        entity.Uom = dto.Uom;
        entity.InstrumentTag = dto.InstrumentTag;
        entity.Enabled = dto.Enabled;
        entity.Compressing = dto.Compressing;
        entity.CompressionMinTime = dto.CompressionMinTime;
        entity.CompressionMaxTime = dto.CompressionMaxTime;
        entity.DigitalSet = dto.DigitalSet;
        entity.Step = dto.Step;
        entity.ValueCalculation = dto.ValueCalculation;
        entity.TimeCalculation = dto.TimeCalculation;
        entity.ErrorCalculation = dto.ErrorCalculation;

        _rtdsMetaContext.SaveChanges();
    }

    /// <summary>
    /// Retrieves tags associated with a specific interface ID.
    /// </summary>
    /// <param name="requestInterfaceId">The ID of the interface to filter tags by.</param>
    /// <returns>An enumerable collection of <see cref="TagEntity"/> objects matching the specified interface ID.</returns>
    public IEnumerable<TagEntity> GetTagsByInterfaceId(uint requestInterfaceId)
    {
        return _rtdsMetaContext.Tags
            .Where(x => x.InterfaceId == requestInterfaceId).AsNoTracking();
    }
}