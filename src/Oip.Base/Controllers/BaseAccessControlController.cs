using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Data.Constants;
using Oip.Base.Exceptions;
using Oip.Base.Security.Objects;
using Oip.Base.Security.Objects.Dtos;

namespace Oip.Base.Controllers;

/// <summary>
/// Access control API of the securable objects of a service. A service derives a controller with its own route.
/// </summary>
[ApiController]
[Authorize]
public abstract class BaseAccessControlController(IObjectAccessService access, ISubjectDirectory subjectDirectory)
    : ControllerBase
{
    private const int SubjectSearchLimit = 50;

    /// <summary>
    /// Gets the securable object types and their rights.
    /// </summary>
    [HttpGet("get-object-types")]
    [ProducesResponseType<List<AccessObjectTypeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    public ActionResult<List<AccessObjectTypeDto>> GetObjectTypes() => access.GetObjectTypes().ToList();

    /// <summary>
    /// Gets the explicit and inherited rights of an object. Requires the manage-acl right on the object.
    /// </summary>
    /// <param name="objectType">Code of the object type.</param>
    /// <param name="objectId">Identifier of the object.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("get-object-acl-by-object")]
    [ProducesResponseType<ObjectAclDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ObjectAclDto>> GetObjectAclByObject([FromQuery] string objectType,
        [FromQuery] long objectId, CancellationToken cancellationToken = default)
    {
        var acl = await access.GetAclAsync(ToRef(objectType, objectId), cancellationToken);
        var subjects = await subjectDirectory.DescribeAsync(
            acl.Entries.Concat(acl.InheritedEntries).Select(e => e.SubjectKey).ToList(), cancellationToken);

        foreach (var entry in acl.Entries.Concat(acl.InheritedEntries))
        {
            if (subjects.TryGetValue(entry.SubjectKey, out var subject))
                entry.DisplayName = subject.DisplayName;
            else
                entry.IsKnown = false;
        }

        return acl;
    }

    /// <summary>
    /// Replaces the explicit rights of an object. Requires the manage-acl right on the object.
    /// </summary>
    /// <param name="request">New explicit rights.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPut("update-object-acl")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateObjectAcl([FromBody] UpdateObjectAclRequest request,
        CancellationToken cancellationToken = default)
    {
        var entries = request.Entries
            .Select(e => new AclEntry(e.SubjectKey, e.Allow, e.Deny))
            .ToList();
        await access.SetAclAsync(ToRef(request.ObjectType, request.ObjectId), request.InheritanceBroken, entries,
            cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Gets the rights of the current user on an object.
    /// </summary>
    /// <param name="objectType">Code of the object type.</param>
    /// <param name="objectId">Identifier of the object.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("get-effective-rights-by-object")]
    [ProducesResponseType<List<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<string>>> GetEffectiveRightsByObject([FromQuery] string objectType,
        [FromQuery] long objectId, CancellationToken cancellationToken = default)
    {
        return (await access.GetEffectiveRightsAsync(ToRef(objectType, objectId), cancellationToken)).ToList();
    }

    /// <summary>
    /// Finds subjects to give rights to on an object. Requires the manage-acl right on the object.
    /// </summary>
    /// <param name="objectType">Code of the object type.</param>
    /// <param name="objectId">Identifier of the object.</param>
    /// <param name="filter">Text contained in the subject name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("get-subjects-by-filter")]
    [ProducesResponseType<List<SubjectDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<SubjectDto>>> GetSubjectsByFilter([FromQuery] string objectType,
        [FromQuery] long objectId, [FromQuery] string? filter = null, CancellationToken cancellationToken = default)
    {
        var obj = ToRef(objectType, objectId);
        if (!await access.HasRightAsync(obj, SecurityConstants.ManageAcl, cancellationToken))
            throw new ApiException("Forbidden", $"The '{SecurityConstants.ManageAcl}' right on {obj} is required.",
                StatusCodes.Status403Forbidden);

        return (await subjectDirectory.SearchAsync(filter, SubjectSearchLimit, cancellationToken)).ToList();
    }

    private ObjectRef ToRef(string objectType, long objectId)
    {
        if (string.IsNullOrWhiteSpace(objectType) || !access.IsRegistered(objectType))
            throw new ApiException("Unknown object type", $"Object type '{objectType}' is not registered.",
                StatusCodes.Status404NotFound);
        if (objectId is <= 0 or > ObjectRef.MaxClientSafeId)
            throw new ApiException("Invalid object identifier",
                $"Object identifier must be between 1 and {ObjectRef.MaxClientSafeId}.", StatusCodes.Status400BadRequest);
        return new ObjectRef(objectType, objectId);
    }
}
