namespace Oip.Rts.Base.Contexts;

internal static class QueryConstants
{
    public const string SelectFromDefaultTagWhereNameLikeFilter = "SELECT * FROM meta.Tag WHERE Name LIKE @filter";
    
    public const string GetMaxTagIdSql = "SELECT max(TagId) + 1 FROM meta.Tag";

    public const string InsertTagSql = @"
        INSERT INTO meta.Tag (
            TagId, Name, ValueType, Source, Descriptor, EngUnits, InstrumentTag,
            Archiving, Compressing, ExcDev, ExcMin, ExcMax, CompDev, CompMin, CompMax,
            Zero, Span, Location1, Location2, Location3, Location4, Location5,
            ExDesc, Scan, DigitalSet, Step, 
            UserInt1, UserInt2, UserInt3, UserInt4, UserInt5,
            UserReal1, UserReal2, UserReal3, UserReal4, UserReal5,
            CreationDate, Creator
        ) VALUES (
            @TagId, @Name, @ValueType, @Source, @Descriptor, @EngUnits, @InstrumentTag,
            @Archiving, @Compressing, @ExcDev, @ExcMin, @ExcMax, @CompDev, @CompMin, @CompMax,
            @Zero, @Span, @Location1, @Location2, @Location3, @Location4, @Location5,
            @ExDesc, @Scan, @DigitalSet, @Step,
            @UserInt1, @UserInt2, @UserInt3, @UserInt4, @UserInt5,
            @UserReal1, @UserReal2, @UserReal3, @UserReal4, @UserReal5,
            @CreationDate, @Creator
        )";
    
    public const string CheckTagNameSql = "SELECT count(TagId) FROM meta.Tag where Name LIKE @tagName";

    
    public const string CreateTagTableSql = @"
CREATE TABLE IF NOT EXISTS data.`{0}`
(
    Time DateTime64(3),
    Value Nullable({1}),
    Status {2}
)
ENGINE = MergeTree
{3}   
    ORDER BY Time;
";
}