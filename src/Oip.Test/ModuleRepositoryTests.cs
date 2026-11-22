using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Oip.Base.Controllers;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Contexts;
using Oip.Base.Data.Dtos;
using Oip.Base.Data.Entities;
using Oip.Base.Data.Extensions;
using Oip.Base.Data.Repositories;
using Oip.Base.Security.Objects;
using Microsoft.Extensions.DependencyInjection;

namespace Oip.Test;

public class ModuleRepositoryTests
{
    [Test]
    public void SubjectsOf_Principal_TakesRolesAndUserOnly()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Role, "operator"),
            new Claim("sub", "alice"),
            new Claim("client_id", "oip-service"),
            new Claim("groups", "plant-1")
        ], "test"));

        var subjects = ModuleAccessControl.SubjectsOf(principal);

        Assert.That(subjects.Keys, Is.EquivalentTo(new[]
            { SubjectKeys.Everyone, SubjectKeys.Role("operator"), SubjectKeys.User("alice") }));
        Assert.That(subjects.UserSubject, Is.EqualTo("alice"));
        Assert.That(ModuleAccessControl.SubjectsOf(new ClaimsPrincipal(new ClaimsIdentity())),
            Is.SameAs(SubjectSet.Anonymous));
    }

    [Test]
    public async Task DeleteModuleInstance_RemovesChildren()
    {
        await using var context = CreateContext();
        var repository = new ModuleRepository(context);

        var module = new ModuleEntity { Name = "Folder", Settings = string.Empty };
        var parent = new ModuleInstanceEntity
        {
            Module = module,
            Label = "Parent",
            Settings = string.Empty,
            Order = 0
        };
        var child = new ModuleInstanceEntity
        {
            Module = module,
            Parent = parent,
            Label = "Child",
            Settings = string.Empty,
            Order = 0
        };
        var grandChild = new ModuleInstanceEntity
        {
            Module = module,
            Parent = child,
            Label = "Grand child",
            Settings = string.Empty,
            Order = 0
        };
        var sibling = new ModuleInstanceEntity
        {
            Module = module,
            Label = "Sibling",
            Settings = string.Empty,
            Order = 1
        };

        context.ModuleInstances.AddRange(parent, child, grandChild, sibling);
        await context.SaveChangesAsync();

        await repository.DeleteModuleInstance(parent.ModuleInstanceId);

        var remainingInstances = await context.ModuleInstances
            .OrderBy(x => x.Order)
            .Select(x => x.Label)
            .ToListAsync();
        var remainingSiblingOrder = await context.ModuleInstances
            .Where(x => x.Label == "Sibling")
            .Select(x => x.Order)
            .SingleAsync();

        Assert.That(remainingInstances, Is.EqualTo(new[] { "Sibling" }));
        Assert.That(remainingSiblingOrder, Is.EqualTo(0));
    }

    [Test]
    public async Task SetStartModule_ReplacesPreviousChoice()
    {
        await using var context = CreateContext();
        var repository = new ModuleRepository(context);

        var module = new ModuleEntity { Name = "Folder", Settings = string.Empty };
        var first = new ModuleInstanceEntity { Module = module, Label = "First", Settings = string.Empty };
        var second = new ModuleInstanceEntity { Module = module, Label = "Second", Settings = string.Empty };

        context.ModuleInstances.AddRange(first, second);
        await context.SaveChangesAsync();

        await repository.SetStartModule("user-sub", first.ModuleInstanceId);
        await repository.SetStartModule("user-sub", second.ModuleInstanceId);

        var startModules = await context.UserStartModules.ToListAsync();

        Assert.That(startModules.Count, Is.EqualTo(1));
        Assert.That(await repository.GetStartModuleInstanceId("user-sub"), Is.EqualTo(second.ModuleInstanceId));
        Assert.That(await repository.GetStartModuleInstanceId("other-sub"), Is.Null);
    }

    [Test]
    public async Task SetStartModule_ThrowsForFolder()
    {
        await using var context = CreateContext();
        var repository = new ModuleRepository(context);

        var folder = new ModuleEntity { Name = ModuleConstants.FolderModuleName };
        var instance = new ModuleInstanceEntity { Module = folder, Label = "Root", Settings = null };
        context.ModuleInstances.Add(instance);
        await context.SaveChangesAsync();

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            repository.SetStartModule("user-sub", instance.ModuleInstanceId));
        Assert.That(await context.UserStartModules.AnyAsync(), Is.False);
    }

    [Test]
    public void SetStartModule_ThrowsForUnknownModuleInstance()
    {
        var context = CreateContext();
        var repository = new ModuleRepository(context);

        Assert.ThrowsAsync<KeyNotFoundException>(() => repository.SetStartModule("user-sub", 42));
    }

    [Test]
    public async Task DeleteModuleInstance_RemovesStartModuleOfEveryUser()
    {
        await using var context = CreateContext();
        var repository = new ModuleRepository(context);

        var module = new ModuleEntity { Name = "Folder", Settings = string.Empty };
        var parent = new ModuleInstanceEntity { Module = module, Label = "Parent", Settings = string.Empty };
        var child = new ModuleInstanceEntity
        {
            Module = module,
            Parent = parent,
            Label = "Child",
            Settings = string.Empty
        };

        context.ModuleInstances.AddRange(parent, child);
        await context.SaveChangesAsync();

        await repository.SetStartModule("first-sub", parent.ModuleInstanceId);
        await repository.SetStartModule("second-sub", child.ModuleInstanceId);

        await repository.DeleteModuleInstance(parent.ModuleInstanceId);

        Assert.That(await context.UserStartModules.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task DeleteStartModule_ClearsOnlyTheGivenUser()
    {
        await using var context = CreateContext();
        var repository = new ModuleRepository(context);

        var module = new ModuleEntity { Name = "Folder", Settings = string.Empty };
        var instance = new ModuleInstanceEntity { Module = module, Label = "Instance", Settings = string.Empty };

        context.ModuleInstances.Add(instance);
        await context.SaveChangesAsync();

        await repository.SetStartModule("first-sub", instance.ModuleInstanceId);
        await repository.SetStartModule("second-sub", instance.ModuleInstanceId);

        await repository.DeleteStartModule("first-sub");

        Assert.That(await repository.GetStartModuleInstanceId("first-sub"), Is.Null);
        Assert.That(await repository.GetStartModuleInstanceId("second-sub"), Is.EqualTo(instance.ModuleInstanceId));
    }

    [Test]
    public async Task CopyModuleInstance_ThrowsForInstanceWithChildren()
    {
        await using var context = CreateContext();
        var repository = new ModuleRepository(context);

        var module = new ModuleEntity { Name = "Folder", Settings = string.Empty };
        var parent = new ModuleInstanceEntity { Module = module, Label = "Parent", Settings = string.Empty };
        var child = new ModuleInstanceEntity
        {
            Module = module,
            Parent = parent,
            Label = "Child",
            Settings = string.Empty
        };

        context.ModuleInstances.AddRange(parent, child);
        await context.SaveChangesAsync();

        Assert.ThrowsAsync<InvalidOperationException>(() => repository.CopyModuleInstance(parent.ModuleInstanceId));
        Assert.That(await context.ModuleInstances.CountAsync(), Is.EqualTo(2));
    }

    [Test]
    public void CopyModuleInstance_ThrowsForUnknownModuleInstance()
    {
        var context = CreateContext();
        var repository = new ModuleRepository(context);

        Assert.ThrowsAsync<KeyNotFoundException>(() => repository.CopyModuleInstance(42));
    }

    [Test]
    public async Task GetModules_ReturnsModuleIcon()
    {
        await using var context = CreateContext();
        var repository = new ModuleRepository(context);

        context.Modules.AddRange(
            new ModuleEntity { Name = "Dashboard", Icon = "pi pi-chart-bar" },
            new ModuleEntity { Name = "Folder" });
        await context.SaveChangesAsync();

        var modules = (await repository.GetModules()).ToList();

        Assert.That(modules.Select(x => x.Value), Is.EqualTo(new[] { "Dashboard", "Folder" }));
        Assert.That(modules[0].Icon, Is.EqualTo("pi pi-chart-bar"));
        Assert.That(modules[1].Icon, Is.Null);
    }

    [Test]
    public async Task ExtensionModule_StoresManifestIcon()
    {
        await using var context = CreateContext();
        var repository = new ModuleRepository(context);
        var manifest = new ExtensionModuleManifestDto
        {
            Key = "reports",
            Name = "Reports",
            Version = "1.0.0",
            ElementName = "oip-reports",
            ScriptUrl = "https://extensions.local/reports.js",
            ApiBaseUrl = "https://extensions.local/api",
            Icon = "pi pi-file"
        };

        var registered = await repository.RegisterExtensionModule(manifest, "https://extensions.local/manifest.json");
        Assert.That((await context.Modules.SingleAsync()).Icon, Is.EqualTo("pi pi-file"));

        manifest.Icon = "pi pi-chart-line";
        await repository.UpdateExtensionModule(registered.ModuleId, manifest, "https://extensions.local/manifest.json");
        Assert.That((await context.Modules.SingleAsync()).Icon, Is.EqualTo("pi pi-chart-line"));
    }

    [Test]
    public async Task AddModuleInstance_AllowsFolderAtRootAndAnyModuleInsideFolder()
    {
        await using var context = CreateContext();
        var repository = new ModuleRepository(context);

        var folder = new ModuleEntity { Name = ModuleConstants.FolderModuleName };
        var page = new ModuleEntity { Name = "Page" };
        context.Modules.AddRange(folder, page);
        await context.SaveChangesAsync();

        await repository.AddModuleInstance(new AddModuleInstanceDto(folder.ModuleId, "Root", null, null, null));
        var root = await context.ModuleInstances.SingleAsync();
        await repository.AddModuleInstance(
            new AddModuleInstanceDto(folder.ModuleId, "Nested", null, root.ModuleInstanceId, null));
        await repository.AddModuleInstance(
            new AddModuleInstanceDto(page.ModuleId, "Page", null, root.ModuleInstanceId, null));

        var children = await context.ModuleInstances
            .Where(x => x.ParentId == root.ModuleInstanceId)
            .OrderBy(x => x.Order)
            .Select(x => x.Label)
            .ToListAsync();

        Assert.That(children, Is.EqualTo(new[] { "Nested", "Page" }));
    }

    [Test]
    public async Task CopyModuleInstance_CopiesSettingsAndRightsBelowOriginal()
    {
        using var env = new ModuleAclEnvironment();
        var context = env.Context;

        var module = new ModuleEntity { Name = "Dashboard", Settings = string.Empty };
        var folder = new ModuleInstanceEntity { Module = module, Label = "Folder", Settings = string.Empty };
        var first = new ModuleInstanceEntity
        {
            Module = module,
            Parent = folder,
            Label = "First",
            Icon = "pi pi-chart-bar",
            Url = "https://example.com",
            Target = "_blank",
            Settings = """{"widgets":[1,2,3]}""",
            Order = 0
        };
        var second = new ModuleInstanceEntity
        {
            Module = module,
            Parent = folder,
            Label = "Second",
            Settings = string.Empty,
            Order = 1
        };

        context.ModuleInstances.AddRange(folder, first, second);
        await context.SaveChangesAsync();
        await env.Access.SetAclUncheckedAsync(ModuleAccessControl.Instance(first.ModuleInstanceId), true,
        [
            new AclEntry(SubjectKeys.Role("viewer"), [SecurityConstants.Read]),
            new AclEntry(SubjectKeys.Role("editor"), [SecurityConstants.Edit])
        ]);

        var copyId = await env.Repository.CopyModuleInstance(first.ModuleInstanceId);

        var copy = await context.ModuleInstances.AsNoTracking().SingleAsync(x => x.ModuleInstanceId == copyId);
        var copyAcl = await env.Access.GetAclUncheckedAsync(ModuleAccessControl.Instance(copyId));
        var order = await context.ModuleInstances
            .Where(x => x.ParentId == folder.ModuleInstanceId)
            .OrderBy(x => x.Order)
            .Select(x => x.Label)
            .ToListAsync();

        Assert.That(copyId, Is.Not.EqualTo(first.ModuleInstanceId));
        Assert.That(copy.ModuleId, Is.EqualTo(first.ModuleId));
        Assert.That(copy.Label, Is.EqualTo("First (copy)"));
        Assert.That(copy.Icon, Is.EqualTo(first.Icon));
        Assert.That(copy.Url, Is.EqualTo(first.Url));
        Assert.That(copy.Target, Is.EqualTo(first.Target));
        Assert.That(copy.Settings, Is.EqualTo(first.Settings));
        Assert.That(copyAcl.InheritanceBroken, Is.True);
        Assert.That(copyAcl.Entries.Select(x => (x.SubjectKey, string.Join(",", x.Allow))),
            Is.EquivalentTo(new[] { ("role:viewer", "read"), ("role:editor", "edit") }));
        Assert.That(order, Is.EqualTo(new[] { "First", "First (copy)", "Second" }));
    }

    [Test]
    public async Task CopyModuleInstance_DoesNotShareSettingsWithOriginal()
    {
        using var env = new ModuleAclEnvironment();
        var repository = env.Repository;

        var module = new ModuleEntity { Name = "Dashboard", Settings = string.Empty };
        var original = new ModuleInstanceEntity { Module = module, Label = "Original", Settings = "{\"a\":1}" };

        env.Context.ModuleInstances.Add(original);
        await env.Context.SaveChangesAsync();

        var copyId = await repository.CopyModuleInstance(original.ModuleInstanceId);
        repository.UpdateModuleInstanceSettings(copyId, "{\"a\":2}");

        Assert.That(repository.GetModuleInstanceSettings(original.ModuleInstanceId), Is.EqualTo("{\"a\":1}"));
        Assert.That(repository.GetModuleInstanceSettings(copyId), Is.EqualTo("{\"a\":2}"));
    }

    [Test]
    public async Task AddModuleInstance_InheritsTheRightsOfTheFolder()
    {
        using var env = new ModuleAclEnvironment();
        var (context, repository) = (env.Context, env.Repository);

        var folder = new ModuleEntity { Name = ModuleConstants.FolderModuleName };
        var page = new ModuleEntity { Name = "Page" };
        context.Modules.AddRange(folder, page);
        await context.SaveChangesAsync();

        await repository.AddModuleInstance(new AddModuleInstanceDto(folder.ModuleId, "Root", null, null, null));
        var root = await context.ModuleInstances.SingleAsync();
        await repository.UpdateInstanceSecurity(root.ModuleInstanceId,
            [new ModuleSecurityDto { Right = SecurityConstants.Read, Role = "operator" }]);
        await repository.AddModuleInstance(
            new AddModuleInstanceDto(page.ModuleId, "Page", null, root.ModuleInstanceId, null));
        var instance = await context.ModuleInstances.SingleAsync(x => x.Label == "Page");

        Assert.That(await repository.GetUserRightsByInstanceId(instance.ModuleInstanceId, ["operator"]),
            Is.EqualTo(new[] { SecurityConstants.Read }));
        Assert.That(await repository.HasInstanceRight(instance.ModuleInstanceId, ["guest"], SecurityConstants.Read),
            Is.False);
        Assert.That(await repository.GetUserRightsByInstanceId(instance.ModuleInstanceId,
                [SecurityConstants.AdminRole]),
            Is.SupersetOf(new[]
            {
                SecurityConstants.Read, SecurityConstants.Edit, SecurityConstants.Delete, SecurityConstants.ManageAcl
            }));
        Assert.That(await repository.GetSecurityByInstanceId(instance.ModuleInstanceId), Is.Empty);
        Assert.That(await context.ModuleInstanceSecurities.AnyAsync(), Is.False);
    }

    [Test]
    public async Task UpdateInstanceSecurity_ReplacesRoleRights_AndKeepsOtherEntries()
    {
        using var env = new ModuleAclEnvironment();
        var instance = await env.AddInstanceAsync("Page");
        var obj = ModuleAccessControl.Instance(instance.ModuleInstanceId);
        await env.Access.SetAclUncheckedAsync(obj, true,
        [
            new AclEntry(SubjectKeys.User("alice"), [SecurityConstants.Edit]),
            new AclEntry(SubjectKeys.Role("old"), [SecurityConstants.Read]),
            new AclEntry(SubjectKeys.Role("operator"), [], [SecurityConstants.Delete])
        ]);

        await env.Repository.UpdateInstanceSecurity(instance.ModuleInstanceId,
        [
            new ModuleSecurityDto { Right = SecurityConstants.Read, Role = "operator" },
            new ModuleSecurityDto { Right = "view-financials", Role = "operator" }
        ]);

        var acl = await env.Access.GetAclUncheckedAsync(obj);
        Assert.That(acl.InheritanceBroken, Is.True);
        Assert.That(acl.Entries.Select(x => (x.SubjectKey, string.Join(",", x.Allow), string.Join(",", x.Deny))),
            Is.EquivalentTo(new[]
            {
                ("user:alice", "edit", ""),
                ("role:operator", "read,view-financials", "delete")
            }));
        Assert.That((await env.Repository.GetSecurityByInstanceId(instance.ModuleInstanceId))
                .Select(x => (x.Role, x.Right)),
            Is.EquivalentTo(new[] { ("operator", "read"), ("operator", "view-financials") }));
    }

    [Test]
    public async Task EditModuleInstance_GrantsReadToExactlyTheViewRoles()
    {
        using var env = new ModuleAclEnvironment();
        var instance = await env.AddInstanceAsync("Page");
        await env.Repository.UpdateInstanceSecurity(instance.ModuleInstanceId,
        [
            new ModuleSecurityDto { Right = SecurityConstants.Read, Role = "old" },
            new ModuleSecurityDto { Right = SecurityConstants.Edit, Role = "old" }
        ]);

        await env.Repository.EditModuleInstance(new EditModuleInstanceDto(instance.ModuleInstanceId, "Renamed",
            null, null, ["operator"], null));

        var menu = (await env.Repository.GetAdminMenu()).Single();
        Assert.That(menu.Label, Is.EqualTo("Renamed"));
        Assert.That(menu.Securities, Is.EqualTo(new[] { "operator" }));
        Assert.That((await env.Repository.GetSecurityByInstanceId(instance.ModuleInstanceId))
                .Select(x => (x.Role, x.Right)),
            Is.EquivalentTo(new[] { ("old", "edit"), ("operator", "read") }));
    }

    [Test]
    public async Task GetModuleForMenuAll_ShowsReadableInstancesOnly()
    {
        using var env = new ModuleAclEnvironment();
        var module = new ModuleEntity { Name = "Remote", Kind = ModuleKind.Extension };
        var folder = new ModuleInstanceEntity { Module = module, Label = "Folder", Settings = string.Empty };
        var open = new ModuleInstanceEntity { Module = module, Parent = folder, Label = "Open", Settings = string.Empty };
        var closed = new ModuleInstanceEntity { Module = module, Parent = folder, Label = "Closed", Settings = string.Empty };
        env.Context.ModuleInstances.AddRange(folder, open, closed);
        await env.Context.SaveChangesAsync();
        await env.Access.SetAclUncheckedAsync(ModuleAccessControl.Instance(folder.ModuleInstanceId), false,
            [new AclEntry(SubjectKeys.Role("operator"), [SecurityConstants.Read])]);
        await env.Access.SetAclUncheckedAsync(ModuleAccessControl.Instance(closed.ModuleInstanceId), true, []);
        await env.Access.SetAclUncheckedAsync(ModuleAccessControl.Instance(open.ModuleInstanceId), false,
            [new AclEntry(SubjectKeys.User("bob"), [SecurityConstants.Edit])]);

        var operatorMenu = (await env.Repository.GetModuleForMenuAll(["operator"], "alice")).Single();
        var guestMenu = await env.Repository.GetModuleForMenuAll(["guest"], "bob");

        Assert.That(operatorMenu.Label, Is.EqualTo("Folder"));
        Assert.That(operatorMenu.Items!.Select(x => x.Label), Is.EqualTo(new[] { "Open" }));
        Assert.That(guestMenu, Is.Empty);
    }

    [Test]
    public async Task ModuleSecurityMigration_KeepsTheAccessOfEveryInstance()
    {
        using var env = new ModuleAclEnvironment(withInterceptor: false);
        var context = env.Context;
        var module = new ModuleEntity { Name = "Page" };
        var folder = new ModuleInstanceEntity
        {
            Module = module, Label = "Folder", Settings = string.Empty,
            Securities = [new ModuleInstanceSecurityEntity { Right = SecurityConstants.Read, Role = "operator" }]
        };
        var page = new ModuleInstanceEntity
        {
            Module = module, Parent = folder, Label = "Page", Settings = string.Empty,
            Securities =
            [
                new ModuleInstanceSecurityEntity { Right = SecurityConstants.Read, Role = "viewer" },
                new ModuleInstanceSecurityEntity { Right = "view-financials", Role = "viewer" }
            ]
        };
        context.ModuleInstances.AddRange(folder, page);
        await context.SaveChangesAsync();

        var migrated = await ModuleSecurityMigration.MigrateAsync(context, env.Access, ["export"]);
        var again = await ModuleSecurityMigration.MigrateAsync(context, env.Access, ["export"]);

        var pageAcl = await env.Access.GetAclUncheckedAsync(ModuleAccessControl.Instance(page.ModuleInstanceId));
        Assert.That(migrated, Is.EqualTo(2));
        Assert.That(again, Is.Zero);
        Assert.That(pageAcl.InheritanceBroken, Is.True);
        Assert.That(pageAcl.ParentId, Is.EqualTo(folder.ModuleInstanceId));
        Assert.That(await env.Repository.GetUserRightsByInstanceId(page.ModuleInstanceId, ["viewer"]),
            Is.EquivalentTo(new[] { SecurityConstants.Read, "view-financials" }));
        Assert.That(await env.Repository.HasInstanceRight(page.ModuleInstanceId, ["operator"], SecurityConstants.Read),
            Is.False);
        Assert.That(await env.Repository.HasInstanceRight(folder.ModuleInstanceId, ["operator"], SecurityConstants.Read),
            Is.True);
        Assert.That(env.Access.Registry.Get(ModuleAccessControl.ModuleInstanceType).CustomRightIds.ContainsKey("export"),
            Is.True);
    }

    [Test]
    public async Task GetAdminMenu_MarksFolderInstances()
    {
        using var env = new ModuleAclEnvironment();
        var (context, repository) = (env.Context, env.Repository);

        var folder = new ModuleEntity { Name = ModuleConstants.FolderModuleName };
        var page = new ModuleEntity { Name = "Page" };
        var root = new ModuleInstanceEntity { Module = folder, Label = "Root", Settings = string.Empty };
        var child = new ModuleInstanceEntity { Module = page, Parent = root, Label = "Child", Settings = null };
        context.ModuleInstances.AddRange(root, child);
        await context.SaveChangesAsync();

        var menu = (await repository.GetAdminMenu()).Single();
        var modules = (await repository.GetModules()).ToList();

        Assert.That(menu.IsFolder, Is.True);
        Assert.That(menu.Items!.Single().IsFolder, Is.False);
        Assert.That(modules.Single(x => x.Value == ModuleConstants.FolderModuleName).IsFolder, Is.True);
        Assert.That(modules.Single(x => x.Value == "Page").IsFolder, Is.False);
    }

    [Test]
    public void SyncModuleSecurities_StoresDeclaredRightsWithAdmin()
    {
        var module = new ModuleEntity
        {
            Name = "Page",
            ModuleSecurities =
            [
                new ModuleSecurityEntity { Right = SecurityConstants.Read, Role = SecurityConstants.AdminRole },
                new ModuleSecurityEntity { Right = "obsolete", Role = SecurityConstants.AdminRole }
            ]
        };

        WebApplicationBuilderExtension.SyncModuleSecurities(module,
        [
            new SecurityResponse { Code = SecurityConstants.Read, Roles = [SecurityConstants.AdminRole] },
            new SecurityResponse { Code = SecurityConstants.Edit, Roles = ["operator"] },
            new SecurityResponse { Code = SecurityConstants.Delete, Roles = null }
        ]);

        Assert.That(module.ModuleSecurities.Select(x => (x.Right, x.Role)), Is.EquivalentTo(new[]
        {
            (SecurityConstants.Read, SecurityConstants.AdminRole),
            (SecurityConstants.Edit, "operator"),
            (SecurityConstants.Edit, SecurityConstants.AdminRole),
            (SecurityConstants.Delete, SecurityConstants.AdminRole)
        }));
    }

    [Test]
    public async Task AddModuleInstance_ThrowsForNonFolderAtRoot()
    {
        await using var context = CreateContext();
        var repository = new ModuleRepository(context);

        var page = new ModuleEntity { Name = "Page" };
        context.Modules.Add(page);
        await context.SaveChangesAsync();

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            repository.AddModuleInstance(new AddModuleInstanceDto(page.ModuleId, "Page", null, null, null)));
        Assert.That(await context.ModuleInstances.AnyAsync(), Is.False);
    }

    [Test]
    public async Task AddModuleInstance_ThrowsForChildOfNonFolder()
    {
        await using var context = CreateContext();
        var repository = new ModuleRepository(context);

        var page = new ModuleEntity { Name = "Page" };
        var parent = new ModuleInstanceEntity { Module = page, Label = "Parent", Settings = string.Empty };
        context.ModuleInstances.Add(parent);
        await context.SaveChangesAsync();

        Assert.ThrowsAsync<InvalidOperationException>(() => repository.AddModuleInstance(
            new AddModuleInstanceDto(page.ModuleId, "Child", null, parent.ModuleInstanceId, null)));
        Assert.That(await context.ModuleInstances.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task AddModuleInstance_ThrowsForUnknownModuleOrParent()
    {
        await using var context = CreateContext();
        var repository = new ModuleRepository(context);

        var folder = new ModuleEntity { Name = ModuleConstants.FolderModuleName };
        context.Modules.Add(folder);
        await context.SaveChangesAsync();

        Assert.ThrowsAsync<KeyNotFoundException>(() =>
            repository.AddModuleInstance(new AddModuleInstanceDto(42, "Unknown", null, null, null)));
        Assert.ThrowsAsync<KeyNotFoundException>(() =>
            repository.AddModuleInstance(new AddModuleInstanceDto(folder.ModuleId, "Orphan", null, 42, null)));
    }

    [Test]
    public async Task GetModuleInstanceSettings_ReturnsDefaultsForNullSettings()
    {
        await using var context = CreateContext();
        var repository = new ModuleRepository(context);

        var folder = new ModuleEntity { Name = ModuleConstants.FolderModuleName };
        var instance = new ModuleInstanceEntity { Module = folder, Label = "Root", Settings = null };
        context.ModuleInstances.Add(instance);
        await context.SaveChangesAsync();

        Assert.That(repository.GetModuleInstanceSettings(instance.ModuleInstanceId), Is.Null);
        Assert.That(repository.GetModuleInstanceSettings<FolderModuleSettings>(instance.ModuleInstanceId).Html,
            Is.Empty);
        Assert.Throws<KeyNotFoundException>(() => repository.GetModuleInstanceSettings(42));
    }

    private static OipModuleContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OipModuleContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new OipModuleContext(options);
    }

    /// <summary>
    /// Module context with the access control of the module instances, as AddOipModuleContext registers it.
    /// </summary>
    private sealed class ModuleAclEnvironment : IDisposable
    {
        private readonly ServiceProvider _provider;
        private readonly IServiceScope _scope;

        public ModuleAclEnvironment(bool withInterceptor = true)
        {
            var services = new ServiceCollection();
            var dbName = Guid.NewGuid().ToString();
            services.AddLogging();
            services.AddDbContext<OipModuleContext>((sp, options) =>
            {
                options.UseInMemoryDatabase(dbName);
                if (withInterceptor)
                    options.UseOipAccessControl<OipModuleContext>(sp);
            });
            services.AddOipAccessControl<OipModuleContext>(ModuleAccessControl.Configure);
            services.AddScoped<ModuleRepository>();
            _provider = services.BuildServiceProvider();
            _scope = _provider.CreateScope();
        }

        public OipModuleContext Context => _scope.ServiceProvider.GetRequiredService<OipModuleContext>();

        public ModuleRepository Repository => _scope.ServiceProvider.GetRequiredService<ModuleRepository>();

        public ObjectAccessService<OipModuleContext> Access =>
            _scope.ServiceProvider.GetRequiredService<ObjectAccessService<OipModuleContext>>();

        public async Task<ModuleInstanceEntity> AddInstanceAsync(string label)
        {
            var instance = new ModuleInstanceEntity
            {
                Module = new ModuleEntity { Name = label }, Label = label, Settings = string.Empty
            };
            Context.ModuleInstances.Add(instance);
            await Context.SaveChangesAsync();
            return instance;
        }

        public void Dispose()
        {
            _scope.Dispose();
            _provider.Dispose();
        }
    }
}
