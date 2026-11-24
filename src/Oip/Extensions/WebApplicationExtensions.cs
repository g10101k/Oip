using Oip.Base.Data.Constants;
using Oip.Base.Security.Objects;
using Oip.Demo.TableQueryDemo;

namespace Oip.Extensions;

/// <summary>
/// Provides extension methods for configuring and initializing web application components.
/// </summary>
public static class WebApplicationExtensions
{
    private const string UserRole = "user";

    /// <summary>
    /// Migrates the demo customer table context by ensuring the database is created and seeding it with initial data.
    /// </summary>
    /// <param name="app">The web application instance used to create a service scope and access required services.</param>
    public static void MigrateDemoCustomerTableContext(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var customerContext = scope.ServiceProvider.GetRequiredService<DemoCustomerTableContext>();
        customerContext.Database.EnsureCreated();
        if (DemoCustomerTableSeeder.Seed(customerContext))
            SeedDemoAccessControl(scope.ServiceProvider.GetRequiredService<ObjectAccessService<DemoCustomerTableContext>>())
                .GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gives every demo category its own rights, so the customer list differs between the admin and the user role.
    /// </summary>
    private static async Task SeedDemoAccessControl(ObjectAccessService<DemoCustomerTableContext> access)
    {
        string[] read = [SecurityConstants.Read];
        string[] readEdit = [SecurityConstants.Read, SecurityConstants.Edit];
        string[] all = [SecurityConstants.Read, SecurityConstants.Edit, SecurityConstants.Delete];
        string[] ownerRights = [SecurityConstants.Edit, SecurityConstants.Delete];

        // Retail: everyone reads, the user role edits, a creator manages own customers.
        await access.SetAclUncheckedAsync(Category(1), false,
        [
            new AclEntry(SubjectKeys.Everyone, read),
            new AclEntry(SubjectKeys.Role(UserRole), readEdit),
            new AclEntry(SubjectKeys.Owner, ownerRights)
        ]);

        // Enterprise: read only for everyone.
        await access.SetAclUncheckedAsync(Category(2), false, [new AclEntry(SubjectKeys.Everyone, read)]);

        // Government: no entries, only the admin role sees these customers.
        await access.SetAclUncheckedAsync(Category(3), false, []);

        // Startup: everyone edits and deletes, except that the user role is denied deleting.
        await access.SetAclUncheckedAsync(Category(4), false,
        [
            new AclEntry(SubjectKeys.Everyone, all),
            new AclEntry(SubjectKeys.Role(UserRole), [], [SecurityConstants.Delete])
        ]);
    }

    private static ObjectRef Category(long id) => new(DemoCustomerTableContext.CategoryObjectType, id);
}
