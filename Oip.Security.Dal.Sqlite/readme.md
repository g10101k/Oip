# Add migration

migration="Init"
dotnet ef migrations add $migration -c AdminAuditLogDbContext -o "Migrations/AdminAuditLog"
dotnet ef migrations add $migration -c AdminIdentityDbContext -o "Migrations/AdminIdentity"
dotnet ef migrations add $migration -c AdminLogDbContext -o "Migrations/AdminLog"
dotnet ef migrations add $migration -c IdentityServerConfigurationDbContext -o "Migrations/IdentityServerConfiguration"
dotnet ef migrations add $migration -c IdentityServerDataProtectionDbContext -o "
Migrations/IdentityServerDataProtection"
dotnet ef migrations add $migration -c IdentityServerPersistedGrantDbContext -o "
Migrations/IdentityServerPersistedGrant"
