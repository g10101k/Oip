using Oip.Demo.TableQueryDemo;

namespace Oip.Extensions;

/// <summary>
/// Provides extension methods for configuring and initializing web application components.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Migrates the demo customer table context by ensuring the database is created and seeding it with initial data.
    /// </summary>
    /// <param name="app">The web application instance used to create a service scope and access required services.</param>
    public static void MigrateDemoCustomerTableContext(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var customerContext = scope.ServiceProvider.GetRequiredService<DemoCustomerTableContext>();
        customerContext.Database.EnsureCreated();
        DemoCustomerTableSeeder.Seed(customerContext);
    }
}