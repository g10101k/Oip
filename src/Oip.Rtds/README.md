# 🏠 Oip.Rtds — Real-Time Data Storage Service

**Oip.Rts** is a high-performance service for collecting, storing, and accessing real-time data, specifically designed
for smart home systems. It allows telemetry from sensors to be stored, tag metadata to be managed, and historical data
to be quickly retrieved for analysis, visualization, or automation.

---

## 🚀 Key Features

- ⚡ Fast data write and read with ClickHouse
- 🏷️ Tag (sensor) management with rich metadata
- 📈 High-precision value storage (up to milliseconds)
- 🧠 Support for multiple data types (float, int, digital, string)
- 🔄 Status quality handling (Good, Bad, Timeout, etc.)
- 📁 Partitioning by year for efficient storage
- 🛡️ SQL-level security (validation and injection protection)

---

## 🧱 Table Structure (Per Tag)

```sql
CREATE TABLE ` tag_12345 `
(
    `time`  DateTime64(3), 
    `value` Nullable(Float32) ,
    `status` Enum(
        'Good' = 0,
        'Questionable' = -1,
        'Substituted' = -2,
        'Interpolated' = -3,
        'NoData' = -4,
        'Bad' = -5,
        'Shutdown' = -6,
        'CalcFailed' = -7,
        'Timeout' = -8,
        'OutOfRange' = -9
    )
) ENGINE = MergeTree
PARTITION BY toYear(time)
ORDER BY time;
```

Tables are automatically created per tag (TagId), based on its type (PointType).
---

## 📦 Tag Model

````csharp
public class Tag
{
    public uint TagId { get; set; }
    public string Name { get; set; }
    public TagTypes PointType { get; set; }
    public string? Descriptor { get; set; }
    public bool? Archiving { get; set; }
    public string Partition { get; set; } = "PARTITION BY toYear(time)";
    // ... other fields
}
````
---

## 🔒 Security
All `INSERT` and `SELECT` queries use Dapper parameters

Table and column names are validated via regex (^[a-zA-Z0-9_]+$)
No raw user input is interpolated into SQL

* Dotnet app start at https://localhost:5003
* Angular client start at https://localhost:50003
