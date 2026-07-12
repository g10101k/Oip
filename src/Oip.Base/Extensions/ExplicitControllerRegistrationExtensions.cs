using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace Oip.Base.Extensions;

internal record MvcBuilderHolder(IMvcBuilder Builder);

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