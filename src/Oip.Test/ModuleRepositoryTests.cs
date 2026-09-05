using Microsoft.EntityFrameworkCore;
using Oip.Base.Data.Contexts;
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

    private static OipModuleContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OipModuleContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new OipModuleContext(options);
    }
}
