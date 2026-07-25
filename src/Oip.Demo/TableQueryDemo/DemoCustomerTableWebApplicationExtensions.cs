using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Oip.Demo.TableQueryDemo;

public static class DemoCustomerTableWebApplicationExtensions
{
    public static void MigrateDemoCustomerTableContext(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var customerContext = scope.ServiceProvider.GetRequiredService<DemoCustomerTableContext>();
        customerContext.Database.EnsureCreated();
        DemoCustomerTableSeeder.Seed(customerContext);
    }
}
