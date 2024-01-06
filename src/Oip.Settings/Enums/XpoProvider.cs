namespace Oip.Settings.Enums;

/// <summary>
/// Enum with available provider
/// </summary>
public enum XpoProvider
{
    /// <summary>
    /// SQLite
    /// </summary>
    SQLite,

    /// <summary>
    /// PostgreSQL
    /// </summary>
    Postgres,

    /// <summary>
    /// MS SQL
    /// </summary>
    MSSqlServer,

    /// <summary>
    /// In Memory (for testing)
    /// </summary>
    InMemoryDataStore
}