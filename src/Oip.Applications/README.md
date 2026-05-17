
## DbMigration

For *nix system:

````shell
migration_name=InitialMigration
dotnet ef migrations add "${migration_name}_Postgres" --verbose --context ApplicationsDbBaseContextPostgres --output-dir Data/Migrations/Postgres -- --ConnectionString="XpoProvider=Postgres;Host=localhost;Port=5432;Database=oip;uid=postgres;pwd=" --UseEfCoreProvider=false
dotnet ef migrations add "${migration_name}_SqlServer" --verbose --context ApplicationsDbBaseContextSqlServer --output-dir Data/Migrations/SqlServer -- --ConnectionString="XpoProvider=MSSqlServer;Server=localhost;Database=oip;uid=sa;Password=" --UseEfCoreProvider=false
````

For windows:

````shell
set migration_name=InitialMigration
dotnet ef migrations add "%migration_name%_Postgres" --context ApplicationsDbBaseContextPostgres --output-dir Data/Migrations/Postgres -- --ConnectionString="XpoProvider=Postgres;Host=localhost;Port=5432;Database=oip;uid=postgres;pwd=" --UseEfCoreProvider=false
dotnet ef migrations add "%migration_name%_SqlServer" --context ApplicationsDbBaseContextSqlServer --output-dir Data/Migrations/SqlServer -- --ConnectionString="XpoProvider=MSSqlServer;Server=localhost;Database=oip;uid=sa;Password=" --UseEfCoreProvider=false
````

