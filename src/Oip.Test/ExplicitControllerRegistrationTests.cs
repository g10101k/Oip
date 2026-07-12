using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Oip.Base.Extensions;

namespace Oip.Test;

[TestFixture]
public class ExplicitControllerRegistrationTests
{
    [Test]
    public void AddControllersAndViewBeforeAddController_FiltersControllers()
    {
        var controllerTypes = DiscoverControllerTypes(services =>
        {
            services.AddControllersAndView();
            services.AddController<RegisteredExplicitTestController>();
        });

        Assert.That(controllerTypes, Contains.Item(typeof(RegisteredExplicitTestController)));
        Assert.That(controllerTypes, Does.Not.Contain(typeof(UnregisteredExplicitTestController)));
    }

    [Test]
    public void AddControllerBeforeAddControllersAndView_FiltersControllers()
    {
        var controllerTypes = DiscoverControllerTypes(services =>
        {
            services.AddController<RegisteredExplicitTestController>();
            services.AddControllersAndView();
        });

        Assert.That(controllerTypes, Contains.Item(typeof(RegisteredExplicitTestController)));
        Assert.That(controllerTypes, Does.Not.Contain(typeof(UnregisteredExplicitTestController)));
    }

    [Test]
    public void AddControllersAndViewWithoutAddController_DoesNotFilterControllers()
    {
        var controllerTypes = DiscoverControllerTypes(services =>
        {
            services.AddControllersAndView();
        });

        Assert.That(controllerTypes, Contains.Item(typeof(RegisteredExplicitTestController)));
        Assert.That(controllerTypes, Contains.Item(typeof(UnregisteredExplicitTestController)));
    }

    private static IReadOnlyCollection<Type> DiscoverControllerTypes(Action<IServiceCollection> configureServices)
    {
        var services = new ServiceCollection();

        configureServices(services);

        var partManager = services
            .Single(x => x.ServiceType == typeof(ApplicationPartManager))
            .ImplementationInstance as ApplicationPartManager;

        Assert.That(partManager, Is.Not.Null);
        partManager!.ApplicationParts.Clear();
        partManager.ApplicationParts.Add(new AssemblyPart(typeof(RegisteredExplicitTestController).Assembly));

        var controllerFeature = new ControllerFeature();
        partManager.PopulateFeature(controllerFeature);

        return controllerFeature.Controllers
            .Select(x => x.AsType())
            .ToArray();
    }
}

public class RegisteredExplicitTestController : ControllerBase;

public class UnregisteredExplicitTestController : ControllerBase;
