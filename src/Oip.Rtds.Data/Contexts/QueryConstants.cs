namespace Oip.Rtds.Data.Contexts;

internal static class QueryConstants
{
    /// <summary>
    /// Number of recently inserted blocks whose hashes a tag value table keeps for insert deduplication.
    /// A retried batch is recognised as a duplicate while it stays inside this window.
    /// </summary>
    public const int DeduplicationWindow = 1000;

    /// <summary>
    /// Creates a tag value table.
    /// {0} - ClickHouse value type, {1} - Status Enum8 definition, {2} - value column codec,
    /// {3} - insert deduplication window.
    /// The Value column is intentionally not Nullable: an absent value is encoded by the Status column
    /// (NoData) while Value keeps a neutral placeholder (0 / NaN).
    /// </summary>
    public const string CreateIntTagValue = @"
CREATE TABLE IF NOT EXISTS data.{0}TagValue
(
    Id UInt32 CODEC(DoubleDelta, ZSTD(1)),
    Time DateTime64(3, 'UTC') CODEC(DoubleDelta, ZSTD(1)),
    Value {0} {2},
    Status {1} CODEC(ZSTD(1))
)
ENGINE = MergeTree
PARTITION BY toYYYYMM(Time)
ORDER BY (Id, Time)
SETTINGS non_replicated_deduplication_window = {3};
";

    /// <summary>
    /// Turns insert deduplication on for a tag value table that already exists.
    /// Tables created before deduplication was introduced keep the setting disabled, so it is applied explicitly.
    /// {0} - ClickHouse value type, {1} - insert deduplication window.
    /// </summary>
    public const string EnableDeduplication = @"
ALTER TABLE data.{0}TagValue MODIFY SETTING non_replicated_deduplication_window = {1};
";

    /// <summary>
    /// Inserts tag values. Values are batched by the caller, so asynchronous inserts are deliberately not used:
    /// they only add a server side buffer copy on top of an already large batch.
    /// {0} - ClickHouse value type, {1} - deduplication token that makes a retried batch idempotent.
    /// </summary>
    public const string InsertIntoQuery = @"
INSERT INTO data.{0}TagValue (Id, Time, Value, Status)
SETTINGS insert_deduplication_token = '{1}'
VALUES";
}
