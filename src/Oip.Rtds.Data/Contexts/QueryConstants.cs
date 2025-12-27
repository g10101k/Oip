namespace Oip.Rtds.Data.Contexts;

internal static class QueryConstants
{
    public const string CreateIntTagValue = @"
CREATE TABLE IF NOT EXISTS data.{0}TagValue
(
    Id UInt32,
    Time DateTime64(3, 'UTC'),    
    Value Nullable({0}),
    Status {1}
)
ENGINE = MergeTree
PARTITION BY toYYYYMM(Time)
ORDER BY (Id, Time);
";

    public const string InsertIntoQuery = @"
INSERT INTO data.{0}TagValue (Id, Time, Value, Status) VALUES {1}";
}