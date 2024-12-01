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
        var query = _db.Features.Include(x => x.FeatureSecurities)
            .Select(e => new FeatureDto()
            {
                FeatureId = e.FeatureId,
                Name = e.Name,
                Settings = e.Settings,
                FeatureSecurities = e.FeatureSecurities.Select(sec => new FeatureSecurityDto()
                {
                    FeatureSecurityId = sec.FeatureSecurityId,
                    Role = sec.Role,
                    Right = sec.Right
                })
            });
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
                    Role = xx.Role,
                    Right = xx.Right
                }).ToList()
            }
        ));
        await _db.SaveChangesAsync();
    }
}