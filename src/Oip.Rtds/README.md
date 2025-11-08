# 🏠 Oip.Rtds — Real-Time Data Storage Service

* Dotnet app start at https://localhost:5003
* Angular client start at https://localhost:50003
* Grcp service start at https://localhost:50004

## DbMigration

For *nix system:

````shell
migration_name=MigrationName
dotnet ef migrations add "${migration_name}_Postgres" --context RtdsMetaContextPostgres --project ./../Oip.Rtds.Data --output-dir Migrations/Postgres -- --ConnectionString="XpoProvider=Postgres;Host=localhost;Port=5432;Database=oip;uid=postgres;pwd=" --UseEfCoreProvider=false
dotnet ef migrations add "${migration_name}_SqlServer" --context RtdsMetaContextSqlServer --project ./../Oip.Rtds.Data --output-dir Migrations/SqlServer -- --ConnectionString="XpoProvider=MSSqlServer;Server=localhost;Database=oip;uid=sa;Password=" --UseEfCoreProvider=false
````

For windows:

````shell
set migration_name=MigrationName
dotnet ef migrations add "%migration_name%_Postgres" --context RtdsMetaContextPostgres --project ./../Oip.Rtds.Data --output-dir Migrations/Postgres -- --ConnectionString="XpoProvider=Postgres;Host=localhost;Port=5432;Database=oip;uid=postgres;pwd=" --UseEfCoreProvider=false
dotnet ef migrations add "%migration_name%_SqlServer" --context RtdsMetaContextSqlServer --project ./../Oip.Rtds.Data --output-dir Migrations/SqlServer -- --ConnectionString="XpoProvider=MSSqlServer;Server=localhost;Database=oip;uid=sa;Password=" --UseEfCoreProvider=false
````

## Why need RtdsMetaContext?

For modules, user and other