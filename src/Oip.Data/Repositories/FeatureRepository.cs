using Microsoft.EntityFrameworkCore;
using Oip.Data.Contexts;
using Oip.Data.Dtos;
using Oip.Data.Entities;

namespace Oip.Data.Repositories;

/// <summary>
/// Feature Repository
/// </summary>
public class FeatureRepository
{
    private readonly OipContext _db;

    /// <summary>
    /// Constructor
    /// </summary>
    public FeatureRepository(OipContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Get all
    /// </summary>
    public async Task<IEnumerable<FeatureDto>> GetAll()
    {
        var query = from feature in _db.Features
            join featureSecurity in _db.FeatureSecurities on feature.FeatureId equals featureSecurity.FeatureId into
                security
            select new FeatureDto()
            {
                FeatureId = feature.FeatureId,
                Settings = feature.Settings,
                Name = feature.Name,
                FeatureSecurities = security.Select(x => new FeatureSecurityDto()
                {
                    Right = x.Right,
                    Role = x.Role
                })
            };
        var result = await query.ToListAsync();
        return result;
    }

    /// <summary>
    /// Insert features
    /// </summary>
    public async Task Insert(IEnumerable<FeatureDto> list)
    {
        await _db.Features.AddRangeAsync(list.Select(x =>
            new FeatureEntity
            {
                Name = x.Name,
                Settings = x.Settings,
                FeatureSecurities = x.FeatureSecurities.Select(xx => new FeatureSecurityEntity()
                {
                    FeatureId = x.FeatureId,
                    Right = xx.Right,
                    Role = xx.Role
                }).ToList()
            }
        ));
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Get all
    /// </summary>
    public async Task<IEnumerable<FeatureInstanceDto>> GetFeatureForMenuAll()
    {
        var list = (from feature in _db.FeaturesInstances.Include(x => x.Items)!.ThenInclude(x => x.Feature)
            where feature.ParentId == null
            select feature).ToList();

        var result = list.Select(ToDto);

        return result;
    }

    private FeatureInstanceDto ToDto(FeatureInstanceEntity feature)
    {
        return new FeatureInstanceDto(feature.FeatureInstanceId, feature.FeatureId, feature.Label, feature.Icon,
            feature.Feature?.RouterLink != null ? [$"{feature.Feature?.RouterLink}{feature.FeatureInstanceId}"] : null, feature.Url,
            feature.Target, feature.Settings, feature.Items?.Select(ToDto).ToList());
    }
}

/// <summary>
/// 
/// </summary>
/// <param name="FeatureInstanceId"></param>
/// <param name="FeatureId"></param>
/// <param name="Label"></param>
/// <param name="Icon"></param>
/// <param name="RouterLink"></param>
/// <param name="Url"></param>
/// <param name="Target"></param>
/// <param name="Settings"></param>
/// <param name="Items">Childs</param>
public record FeatureInstanceDto(
    int FeatureInstanceId,
    int FeatureId,
    string Label,
    string? Icon,
    List<string>? RouterLink,
    string? Url,
    string? Target,
    string? Settings,
    List<FeatureInstanceDto>? Items);