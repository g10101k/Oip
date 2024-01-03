using Microsoft.Extensions.DependencyInjection;

namespace Oip.Security.Dal;

public abstract class OipSecurityOptionsBuilder
{
    protected OipSecurityOptionsBuilder(IServiceCollection services)
    {
    }

    public IServiceCollection Services { get; }
}