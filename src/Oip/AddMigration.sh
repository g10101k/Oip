migration_name=Initialize
dotnet ef migrations add "${migration_name}Postgres" --context PostgresMigrationContext --project ./../Oip.Data --output-dir Migrations/Postgres -- --ConnectionString="XpoProvider=Postgres;Host=localhost;Port=5432;Database=oip;uid=postgres;pwd=" --UseEfCoreProvider=false
dotnet ef migrations add "${migration_name}SqlServer" --context SqlServerMigrationContext --project ./../Oip.Data --output-dir Migrations/MsSqlServer -- --ConnectionString="XpoProvider=MSSqlServer;Server=localhost;Database=oip;uid=sa;Password=" --UseEfCoreProvider=false
