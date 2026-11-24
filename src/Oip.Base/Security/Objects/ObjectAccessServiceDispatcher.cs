using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Oip.Base.Exceptions;
using Oip.Base.Security.Objects.Dtos;

namespace Oip.Base.Security.Objects;

/// <summary>
/// Routes a call to the access control of the context that registered the object type.
/// </summary>
public sealed class ObjectAccessServiceDispatcher(
    IServiceProvider serviceProvider,
    IEnumerable<IAccessControlRegistration> registrations) : IObjectAccessService
{
    private readonly IReadOnlyList<IAccessControlRegistration> _registrations = registrations.ToList();

    /// <inheritdoc />
    public IReadOnlyList<AccessObjectTypeDto> GetObjectTypes() =>
        _registrations.SelectMany(r => Resolve(r).GetObjectTypes()).ToList();

    /// <inheritdoc />
    public bool IsRegistered(string objectType) => _registrations.Any(r => r.Find(objectType) is not null);

    /// <inheritdoc />
    public Task<bool> HasRightAsync(ObjectRef obj, string right, CancellationToken cancellationToken = default) =>
        For(obj.Type).HasRightAsync(obj, right, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> GetEffectiveRightsAsync(ObjectRef obj,
        CancellationToken cancellationToken = default) =>
        For(obj.Type).GetEffectiveRightsAsync(obj, cancellationToken);

    /// <inheritdoc />
    public Task<ObjectAclDto> GetAclAsync(ObjectRef obj, CancellationToken cancellationToken = default) =>
        For(obj.Type).GetAclAsync(obj, cancellationToken);

    /// <inheritdoc />
    public Task SetAclAsync(ObjectRef obj, bool inheritanceBroken, IReadOnlyCollection<AclEntry> entries,
        CancellationToken cancellationToken = default) =>
        For(obj.Type).SetAclAsync(obj, inheritanceBroken, entries, cancellationToken);

    private IObjectAccessService For(string objectType)
    {
        var registration = _registrations.FirstOrDefault(r => r.Find(objectType) is not null)
                           ?? throw new ApiException("Unknown object type",
                               $"Object type '{objectType}' is not registered.", StatusCodes.Status404NotFound);
        return Resolve(registration);
    }

    private IObjectAccessService Resolve(IAccessControlRegistration registration) =>
        (IObjectAccessService)serviceProvider.GetRequiredService(
            typeof(ObjectAccessService<>).MakeGenericType(registration.ContextType));
}
