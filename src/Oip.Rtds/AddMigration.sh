migration_name=Initialize
dotnet ef migrations add "${migration_name}" --verbose --context RtdsMetaContext --project ./../Oip.Rtds.Data.Postgres --output-dir Migrations -- --ConnectionString="XpoProvider=Postgres;Host=localhost;Port=5432;Database=oip;uid=postgres;pwd=" --UseEfCoreProvider=false
dotnet ef migrations add "${migration_name}" --verbose --context RtdsMetaContext --project ./../Oip.Rtds.Data.SqlServer --output-dir Migrations -- --ConnectionString="XpoProvider=MSSqlServer;Server=localhost;Database=oip;uid=sa;Password=" --UseEfCoreProvider=false
