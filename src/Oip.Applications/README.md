# Oip.Applications

Applications registry service for OIP frontend application switching.

# 

````shell
migration_name=AddServiceTypeToApplicationRegistry
dotnet ef migrations add "${migration_name}_Postgres" --verbose --project ../Oip.Applications.Base/Oip.Applications.Base.csproj --context ApplicationRegistryDbContextPostgres --output-dir Data/Migrations/Postgres -- --ConnectionString="XpoProvider=Postgres;Host=localhost;Port=5432;Database=oip;uid=postgres;pwd=" --UseEfCoreProvider=false
dotnet ef migrations add "${migration_name}_SqlServer" --verbose --project ../Oip.Applications.Base/Oip.Applications.Base.csproj --context ApplicationRegistryDbContextSqlServer --output-dir Data/Migrations/SqlServer -- --ConnectionString="XpoProvider=MSSqlServer;Server=localhost;Database=oip;uid=sa;Password=" --UseEfCoreProvider=false
````

For windows:

````shell
set migration_name=InitialMigration
dotnet ef migrations add "%migration_name%_Postgres" --project ../Oip.Applications.Base/Oip.Applications.Base.csproj --context ApplicationRegistryDbContextPostgres --output-dir Data/Migrations/Postgres -- --ConnectionString="XpoProvider=Postgres;Host=localhost;Port=5432;Database=oip;uid=postgres;pwd=" --UseEfCoreProvider=false
dotnet ef migrations add "%migration_name%_SqlServer" --project ../Oip.Applications.Base/Oip.Applications.Base.csproj --context ApplicationRegistryDbContextSqlServer  --output-dir Data/Migrations/SqlServer -- --ConnectionString="XpoProvider=MSSqlServer;Server=localhost;Database=oip;uid=sa;Password=" --UseEfCoreProvider=false
````
