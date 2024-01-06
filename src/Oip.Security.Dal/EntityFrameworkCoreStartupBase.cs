using System;
using Microsoft.EntityFrameworkCore;

namespace Oip.Security.Dal;

public abstract class EntityFrameworkCoreStartupBase
{
    protected abstract string ProviderName { get; }

    public void ConfigureOipSecurity(OipSecurityOptionsBuilder oipSecurity)
    {
    }

    protected virtual string GetDefaultConnectionString()
    {
        throw new Exception($"No connection string specified for the {ProviderName} provider");
    }

    protected abstract void Configure(DbContextOptionsBuilder options, string connectionString);
}