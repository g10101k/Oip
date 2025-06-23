namespace Oip.Base.Controllers.Api;

/// <summary>
/// DTO represents a migration status.
/// </summary>
public class MigrationDto(string name, bool applied, bool pending, bool exist)
{
    /// <summary>
    /// Name of the migration.
    /// </summary>
    public string Name { get; set; } = name;

    /// <summary>
    /// Indicates whether the migration has been applied to the database.
    /// </summary>
    public bool Applied { get; set; } = applied;

    /// <summary>
    /// Indicates whether the migration is pending application.
    /// </summary>
    public bool Pending { get; set; } = pending;

    /// <summary>
    /// Indicates whether the migration exists.
    /// </summary>
    public bool Exist { get; set; } = exist;
}