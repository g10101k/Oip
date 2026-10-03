using Microsoft.EntityFrameworkCore;
using Oip.Base.Data.Contexts;
using Oip.Base.Data.Dtos;
using Oip.Base.Data.Entities;
using Oip.Base.Data.Repositories;

namespace Oip.Test;

public class ModuleRepositoryTests
{
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
    public async Task CopyModuleInstance_CopiesSettingsAndSecuritiesBelowOriginal()
    {
        await using var context = CreateContext();
        var repository = new ModuleRepository(context);

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
            Order = 0,
            Securities =
            [
                new ModuleInstanceSecurityEntity { Right = "read", Role = "admin" },
                new ModuleInstanceSecurityEntity { Right = "edit", Role = "editor" }
            ]
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

        var copyId = await repository.CopyModuleInstance(first.ModuleInstanceId);

        var copy = await context.ModuleInstances
            .Include(x => x.Securities)
            .SingleAsync(x => x.ModuleInstanceId == copyId);
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
        Assert.That(copy.Securities.Select(x => (x.Right, x.Role)),
            Is.EquivalentTo(new[] { ("read", "admin"), ("edit", "editor") }));
        Assert.That(order, Is.EqualTo(new[] { "First", "First (copy)", "Second" }));
    }

    [Test]
    public async Task CopyModuleInstance_DoesNotShareSettingsWithOriginal()
    {
        await using var context = CreateContext();
        var repository = new ModuleRepository(context);

        var module = new ModuleEntity { Name = "Dashboard", Settings = string.Empty };
        var original = new ModuleInstanceEntity { Module = module, Label = "Original", Settings = "{\"a\":1}" };

        context.ModuleInstances.Add(original);
        await context.SaveChangesAsync();

        var copyId = await repository.CopyModuleInstance(original.ModuleInstanceId);
        repository.UpdateModuleInstanceSettings(copyId, "{\"a\":2}");

        Assert.That(repository.GetModuleInstanceSettings(original.ModuleInstanceId), Is.EqualTo("{\"a\":1}"));
        Assert.That(repository.GetModuleInstanceSettings(copyId), Is.EqualTo("{\"a\":2}"));
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

    private static OipModuleContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OipModuleContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new OipModuleContext(options);
    }
}
