# Shell

* Dotnet app start at http://localhost:5002
* Angular client start at http://localhost:50000

## DbMigration

For *nix system:

````shell
migration_name=AlignmentMigration
dotnet ef migrations add "${migration_name}_Postgres" --verbose --context OipModuleContextPostgres --project ./../Oip.Base.Data --output-dir Migrations/Postgres -- --ConnectionString="XpoProvider=Postgres;Host=localhost;Port=5432;Database=oip;uid=postgres;pwd=" --UseEfCoreProvider=false
dotnet ef migrations add "${migration_name}_SqlServer" --verbose --context OipModuleContextSqlServer --project ./../Oip.Base.Data --output-dir Migrations/SqlServer -- --ConnectionString="XpoProvider=MSSqlServer;Server=localhost;Database=oip;uid=sa;Password=" --UseEfCoreProvider=false
````

For windows:

````shell
set migration_name=InitialMigration
dotnet ef migrations add "%migration_name%_Postgres" --context OipModuleContextPostgres --project ./../Oip.Base.Data --output-dir Migrations/Postgres -- --ConnectionString="XpoProvider=Postgres;Host=localhost;Port=5432;Database=oip;uid=postgres;pwd=" --UseEfCoreProvider=false
dotnet ef migrations add "%migration_name%_SqlServer" --context OipModuleContextSqlServer --project ./../Oip.Base.Data --output-dir Migrations/SqlServer -- --ConnectionString="XpoProvider=MSSqlServer;Server=localhost;Database=oip;uid=sa;Password=" --UseEfCoreProvider=false
````
