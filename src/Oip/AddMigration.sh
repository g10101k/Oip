migration_name=Initialization
dotnet ef migrations add $migration_name --context OipContext --project ./../Oip.Data.Sqlite -- --ConnectionString="XpoProvider=SQLite;Data Source=Database/application.db"
dotnet ef migrations add $migration_name --context OipContext --project ./../Oip.Data.Postgres -- --ConnectionString="XpoProvider=Postgres;Host=localhost;Port=5432;Database=Oip;uid=postgres;pwd="
dotnet ef migrations add $migration_name --context OipContext --project ./../Oip.DataData.SqlServer -- --ConnectionString="XpoProvider=MSSqlServer;Server=localhost;Database=Oip;UID=sa;PWD=;TrustServerCertificate=true;"
