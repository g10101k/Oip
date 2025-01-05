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
    public async Task<IEnumerable<FeatureInstanceDto>> GetFeatureForMenuAll(List<string> roles)
    {
        var query = from feature in _db.FeatureInstances
                .Include(x => x.Feature)
            join security in _db.FeatureInstanceSecurities on feature.FeatureInstanceId equals security
                .FeatureInstanceId
            where security.Right == "read" && roles.Contains(security.Role)
            select feature;
        var result = (await query.ToListAsync()).Where(x => x.Parent == null).Select(ToDto);

        return result.ToList();
    }

    /// <summary>
    /// Get instance security by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<List<FeatureSecurityDto>> GetSecurityByInstanceId(int id)
    {
        var query = from security in _db.FeatureInstanceSecurities
            where security.FeatureInstanceId == id
            select new FeatureSecurityDto
            {
                Right = security.Right,
                Role = security.Role
            };

        return await query.ToListAsync();
    }

    /// <summary>
    /// Update
    /// </summary>
    public async Task UpdateInstanceSecurity(int id, IEnumerable<FeatureSecurityDto> security)
    {
        var query =
            from sec in _db.FeatureInstanceSecurities
            where sec.FeatureInstanceId == id
            select sec;
        var list = await query.ToListAsync();

        // delete
        var listToDelete = list.Where(x => !security.Any(s => s.Right == x.Right && s.Role == x.Role)).ToList();
        _db.FeatureInstanceSecurities.RemoveRange(listToDelete);
        // add
        var listToAdd = security.Where(x => !list.Exists(s => s.Right == x.Right && s.Role == x.Role))
            .Select(x => new FeatureInstanceSecurityEntity()
            {
                FeatureInstanceId = id,
                Right = x.Right,
                Role = x.Role
            }).ToList();
        _db.FeatureInstanceSecurities.AddRange(listToAdd);

        await _db.SaveChangesAsync();
    }

    private FeatureInstanceDto ToDto(FeatureInstanceEntity feature)
    {
        return new FeatureInstanceDto()
        {
            FeatureInstanceId = feature.FeatureInstanceId,
            FeatureId = feature.FeatureId,
            Label = feature.Label,
            Icon = feature.Icon,
            RouterLink = feature.Feature.RouterLink != null
                ? [$"{feature.Feature.RouterLink}{feature.FeatureInstanceId}"]
                : null,
            Url = feature.Url,
            Target = feature.Target,
            Settings = feature.Settings,
            Items = feature.Items.Count == 0 ? null : feature.Items.Select(ToDto).ToList()
        };
    }
}