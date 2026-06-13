
## DbMigration

For *nix system:

````shell
migration_name=AddCommentAndPrimaryKeyHook
dotnet ef migrations add "${migration_name}_Postgres" --verbose --project ../Oip.Users.Base/Oip.Users.Base.csproj --context UserContextPostgres --output-dir Data/Migrations/Postgres -- --ConnectionString="XpoProvider=Postgres;Host=localhost;Port=5432;Database=oip;uid=postgres;pwd=" --UseEfCoreProvider=false
dotnet ef migrations add "${migration_name}_SqlServer" --verbose --project ../Oip.Users.Base/Oip.Users.Base.csproj --context UserContextSqlServer --output-dir Data/Migrations/SqlServer -- --ConnectionString="XpoProvider=MSSqlServer;Server=localhost;Database=oip;uid=sa;Password=" --UseEfCoreProvider=false
````

For windows:

````shell
set migration_name=InitialMigration
dotnet ef migrations add "%migration_name%_Postgres" --project ../Oip.Users.Base/Oip.Users.Base.csproj --context UserContextPostgres --output-dir Data/Migrations/Postgres -- --ConnectionString="XpoProvider=Postgres;Host=localhost;Port=5432;Database=oip;uid=postgres;pwd=" --UseEfCoreProvider=false
dotnet ef migrations add "%migration_name%_SqlServer" --project ../Oip.Users.Base/Oip.Users.Base.csproj --context UserContextSqlServer --output-dir Data/Migrations/SqlServer -- --ConnectionString="XpoProvider=MSSqlServer;Server=localhost;Database=oip;uid=sa;Password=" --UseEfCoreProvider=false
````
