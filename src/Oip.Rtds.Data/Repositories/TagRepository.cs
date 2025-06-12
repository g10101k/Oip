using Microsoft.EntityFrameworkCore;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="TagRepository"/> class
    /// </summary>
    /// <param name="rtdsMetaContext">The context for tag metadata operations</param>
    /// <param name="rtdsContext">The context for time-series data operations</param>
    public TagRepository(RtdsMetaContext rtdsMetaContext, RtdsContext rtdsContext)
    {
        _rtdsMetaContext = rtdsMetaContext;
        _rtdsContext = rtdsContext;
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
        var tableName = $"{tag.TagId:D6}";
        var valueType = GetClickHouseTypeFromTagType(tag.ValueType);
        var statusType = GenerateClickHouseEnum8<TagValueStatus>();
        await _rtdsContext.CreateTagTableAsync(tableName, valueType, statusType, tag.Partition);

        _rtdsMetaContext.Tags.Add(new TagEntity
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
            Location1 = tag.Location1,
            Location2 = tag.Location2,
            Location3 = tag.Location3,
            Location4 = tag.Location4,
            Location5 = tag.Location5,
            ExDesc = tag.ExDesc,
            Scan = tag.Scan,
            DigitalSet = tag.DigitalSet,
            Step = tag.Step,
            Future = tag.Future,
            UserInt1 = tag.UserInt1,
            UserInt2 = tag.UserInt2,
            UserInt3 = tag.UserInt3,
            UserInt4 = tag.UserInt4,
            UserInt5 = tag.UserInt5,
            UserReal1 = tag.UserReal1,
            UserReal2 = tag.UserReal2,
            UserReal3 = tag.UserReal3,
            UserReal4 = tag.UserReal4,
            UserReal5 = tag.UserReal5,
            CreationDate = tag.CreationDate,
            Creator = tag.Creator,
            Partition = tag.Partition
        });

        await _rtdsMetaContext.SaveChangesAsync();
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
                Location1 = x.Location1,
                Location2 = x.Location2,
                Location3 = x.Location3,
                Location4 = x.Location4,
                Location5 = x.Location5,
                ExDesc = x.ExDesc,
                Scan = x.Scan,
                DigitalSet = x.DigitalSet,
                Step = x.Step,
                Future = x.Future,
                UserInt1 = x.UserInt1,
                UserInt2 = x.UserInt2,
                UserInt3 = x.UserInt3,
                UserInt4 = x.UserInt4,
                UserInt5 = x.UserInt5,
                UserReal1 = x.UserReal1,
                UserReal2 = x.UserReal2,
                UserReal3 = x.UserReal3,
                UserReal4 = x.UserReal4,
                UserReal5 = x.UserReal5,
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
            TagTypes.Blob => "String", // Можно также использовать FixedString(N) или Base64
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