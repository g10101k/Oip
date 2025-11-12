using System.Reflection;
using System.Runtime.Loader;

namespace Oip.Rtds.Base;

/// <summary>
/// Represents a collectible AssemblyLoadContext used to load and unload dynamically compiled formula assemblies
/// </summary>
public class CollectibleAssemblyLoadContext : AssemblyLoadContext
{
    /// <inheritdoc />
    public CollectibleAssemblyLoadContext() : base(isCollectible: true)
    {
    }

    /// <inheritdoc />
    protected override Assembly Load(AssemblyName assemblyName) => null;
}