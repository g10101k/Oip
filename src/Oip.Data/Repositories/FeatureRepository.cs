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
                Settings = x.Settings
            }
        ));
        await _db.SaveChangesAsync();
    }
}