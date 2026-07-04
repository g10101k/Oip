namespace Oip.Data.Repositories;

/// <summary>
/// Describes a physical extension table that follows a base entity primary key.
/// </summary>
public sealed class ExtensionEntityOptions
{
    /// <summary>
    /// Extension table schema.
    /// </summary>
    public required string Schema { get; init; }

    /// <summary>
    /// Extension table name.
    /// </summary>
    public required string Table { get; init; }

    /// <summary>
    /// Extension table primary key and foreign key column.
    /// </summary>
    public required string KeyColumn { get; init; }

    /// <summary>
    /// Whether the database foreign key cascades delete from base row to extension row.
    /// </summary>
    public bool HasCascadeDelete { get; init; }
}
