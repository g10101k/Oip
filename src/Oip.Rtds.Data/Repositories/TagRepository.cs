using Oip.Rtds.Data.Contexts;
using Oip.Rtds.Data.Dtos;
using Oip.Rtds.Data.Entities;
using Oip.Rtds.Data.Enums;

namespace Oip.Rtds.Data.Repositories;

/// <summary>
/// 
/// </summary>
public class TagRepository
{
    private readonly RtdsMetaContext _rtdsMetaContext;
    private readonly RtdsContext _rtdsContext;

    public TagRepository(RtdsMetaContext rtdsMetaContext, RtdsContext rtdsContext)
    {
        _rtdsMetaContext = rtdsMetaContext;
        _rtdsContext = rtdsContext;
    }

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
    /// Retrieves a list of <see cref="TagEntity"/> objects from the database that match the specified filter.
    /// </summary>
    /// <param name="filter">A string filter to apply to tag names (e.g., "Sensor%").</param>
    /// <returns>A list of <see cref="TagEntity"/> objects matching the filter.</returns>
    public List<TagEntity> GetTagsByFilter(string filter)
    {
        return _connection.Query<TagEntity>(
            QueryConstants.SelectFromDefaultTagWhereNameLikeFilter,
            new { filter }
        ).ToList();
    }
    
    private static string GenerateClickHouseEnum8<TEnum>() where TEnum : Enum
    {
        var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
        var entries = values.Select(v => $"'{v}' = {(int)(object)v}");
        return "Enum8(" + string.Join(", ", entries) + ")";
    }
}