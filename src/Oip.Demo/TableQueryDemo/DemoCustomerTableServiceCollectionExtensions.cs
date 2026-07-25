using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Oip.Demo.TableQueryDemo;

public static class DemoCustomerTableServiceCollectionExtensions
{
    public static IServiceCollection AddDemoCustomerTable(this IServiceCollection services, string databaseName = "CustomerTableDemo")
    {
        services.AddDbContext<DemoCustomerTableContext>(options => options.UseInMemoryDatabase(databaseName));
        return services;
    }
}
