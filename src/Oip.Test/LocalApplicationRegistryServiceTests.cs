using Microsoft.EntityFrameworkCore;
using Moq;
using Oip.Applications.Base.Data.Contexts;
using Oip.Applications.Base.Data.Entities;
using Oip.Applications.Base.Data.Repositories;
using Oip.Applications.Base.Services;
using Oip.Base.Settings;

namespace Oip.Test;

[TestFixture]
public class LocalApplicationRegistryServiceTests
{
    [Test]
    public async Task GetApplicationRegistryItemsAsync_ReturnsOnlyEnabled()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var service = CreateService(context);

        var applications = await service.GetApplicationRegistryItemsAsync();

        Assert.That(applications.Select(x => x.Code), Is.EqualTo(new[] { "enabled" }));
    }

    [Test]
    public async Task GetAllApplicationRegistryItemsAsync_ReturnsDisabledToo()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var service = CreateService(context);

        var applications = await service.GetAllApplicationRegistryItemsAsync();

        Assert.That(applications.Select(x => x.Code), Is.EqualTo(new[] { "disabled", "enabled" }));
    }

    private static async Task SeedAsync(ApplicationRegistryDbContext context)
    {
        context.ApplicationRegistryItems.AddRange(
            CreateEntity("enabled", order: 2, enabled: true),
            CreateEntity("disabled", order: 1, enabled: false));
        await context.SaveChangesAsync();
    }

    private static ApplicationRegistryItemEntity CreateEntity(string code, int order, bool enabled)
    {
        return new ApplicationRegistryItemEntity
        {
            Code = code,
            DisplayName = code,
            BaseUrl = $"https://{code}.invalid",
            InternalBaseUrl = $"https://{code}.internal.invalid",
            Icon = "pi pi-circle",
            Order = order,
            Enabled = enabled
        };
    }

    private static LocalApplicationRegistryService CreateService(ApplicationRegistryDbContext context)
    {
        var settingsMock = new Mock<ISettings>();
        settingsMock.Setup(x => x.Application).Returns(new ApplicationSettings { Code = "current" });
        return new LocalApplicationRegistryService(new ApplicationRegistryRepository(context), settingsMock.Object);
    }

    private static ApplicationRegistryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationRegistryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationRegistryDbContext(options);
    }
}
