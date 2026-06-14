using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace Oip.Base.Extensions;

/// <summary>
/// Provides explicit controller registration for MVC controller discovery.
/// </summary>
public static class ExplicitControllerRegistrationExtensions
{
    /// <summary>
    /// Registers a controller type that should be exposed by ASP.NET Core MVC.
    /// </summary>
    /// <typeparam name="TController">The controller type to expose.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection instance.</returns>
    public static IServiceCollection AddController<TController>(this IServiceCollection services)
        where TController : ControllerBase
    {
        GetOrCreateRegistry(services).Add(typeof(TController));
        return services;
    }

    internal static ExplicitControllerRegistry GetOrCreateRegistry(IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ExplicitControllerRegistry));
        if (descriptor?.ImplementationInstance is ExplicitControllerRegistry existingRegistry)
            return existingRegistry;

        var registry = new ExplicitControllerRegistry();
        services.AddSingleton(registry);
        return registry;
    }
}

internal sealed class ExplicitControllerRegistry
{
    private readonly HashSet<TypeInfo> _controllerTypes = new();

    public IReadOnlySet<TypeInfo> ControllerTypes => _controllerTypes;

    public void Add(Type controllerType)
    {
        _controllerTypes.Add(controllerType.GetTypeInfo());
    }
}

internal sealed class ExplicitControllerFeatureProvider(ExplicitControllerRegistry registry) : ControllerFeatureProvider
{
    protected override bool IsController(TypeInfo typeInfo)
    {
        return registry.ControllerTypes.Contains(typeInfo) && base.IsController(typeInfo);
    }
}
