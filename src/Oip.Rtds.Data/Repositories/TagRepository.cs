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
    /// <param name="tag">The tag creation DTO containing all configuration parameters</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when the specified tag value type is not supported
    /// </exception>
    public async Task AddTag(TagCreateDto tag)
    {
        await _rtdsMetaContext.Database.BeginTransactionAsync();
        try
        {
            var tagEntity = await _rtdsMetaContext.Tags.AddAsync(new TagEntity
            {
                Name = tag.Name,
                ValueType = tag.ValueType,
                Source = tag.Source,
                Descriptor = tag.Descriptor,
                EngUnits = tag.EngUnits,
                InstrumentTag = tag.InstrumentTag,
                Archiving = tag.Archiving,
                Compressing = tag.Compressing,
                ExcDev = tag.ExcDev,
                ExcMin = tag.ExcMin,
                ExcMax = tag.ExcMax,
                CompDev = tag.CompDev,
                CompMin = tag.CompMin,
                CompMax = tag.CompMax,
                Zero = tag.Zero,
                Span = tag.Span,
                ExDesc = tag.ExDesc,
                Scan = tag.Scan,
                DigitalSet = tag.DigitalSet,
                Step = tag.Step,
                CreationDate = DateTimeOffset.UtcNow,
                Creator = _userService.GetUserEmail() ?? throw new InvalidOperationException("User not found"),
                Partition = tag.Partition
            });

            await _rtdsMetaContext.SaveChangesAsync();
            var tableName = $"{tagEntity.Entity.TagId:D6}";
            var valueType = GetClickHouseTypeFromTagType(tag.ValueType);
            var statusType = GenerateClickHouseEnum8<TagValueStatus>();

            await _rtdsContext.CreateTagTableAsync(tableName, valueType, statusType, tag.Partition);
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
    public List<TagGetDto> GetTagsByFilter(string filter)
    {
        return _rtdsMetaContext.Tags
            .Where(tag => EF.Functions.Like(tag.Name, filter))
            .Select(x => new TagGetDto()
            {
                TagId = x.TagId,
                Name = x.Name,
                ValueType = x.ValueType,
                Source = x.Source,
                Descriptor = x.Descriptor,
                EngUnits = x.EngUnits,
                InstrumentTag = x.InstrumentTag,
                Archiving = x.Archiving,
                Compressing = x.Compressing,
                ExcDev = x.ExcDev,
                ExcMin = x.ExcMin,
                ExcMax = x.ExcMax,
                CompDev = x.CompDev,
                CompMin = x.CompMin,
                CompMax = x.CompMax,
                Zero = x.Zero,
                Span = x.Span,
                ExDesc = x.ExDesc,
                Scan = x.Scan,
                DigitalSet = x.DigitalSet,
                Step = x.Step,
                CreationDate = x.CreationDate,
                Creator = x.Creator,
                Partition = x.Partition
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
}