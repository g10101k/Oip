namespace Oip.Settings.Enums;

/// <summary>
/// Enum with available provider
/// </summary>
public enum XpoProvider
{
    /// <summary>
    /// SQLite
    /// </summary>
    // ReSharper disable once InconsistentNaming
    SQLite,

    /// <summary>
    /// PostgreSQL
    /// </summary>
    Postgres,

    /// <summary>
    /// MS SQL
    /// </summary>
    // ReSharper disable once InconsistentNaming
    MSSqlServer,

    /// <summary>
    /// In Memory (for testing)
    /// </summary>
    InMemoryDataStore
}