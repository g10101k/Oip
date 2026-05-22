using Microsoft.EntityFrameworkCore;
using Oip.Data.Contexts;
using Oip.Data.Entities;
using Oip.Data.Repositories;

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

    private static OipModuleContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OipModuleContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new OipModuleContext(options);
    }
}
