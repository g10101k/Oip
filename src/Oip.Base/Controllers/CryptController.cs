using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Exceptions;
using Oip.Base.Services;

namespace Oip.Api.Controllers;

/// <summary>
/// Controller for managing weather forecast data.
/// </summary>
[ApiController]
[Route("api/crypt")]
[ApiExplorerSettings(GroupName = "base")]
public class CryptController(CryptService cryptService) : ControllerBase
{
    /// <summary>
    /// Protects a message using encryption services.
    /// </summary>
    /// <param name="request">The message to be protected</param>
    /// <returns>The protected message</returns>
    [HttpPost("protect")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public ActionResult<CryptResponse> Crypt(CryptRequest request)
    {
        return Ok(new CryptResponse(cryptService.Protect(request.Message)));
    }
}

/// <summary>
/// Request for cryptographic operations with a message to be processed.
/// </summary>
/// <param name="Message">The message to be protected or decrypted.</param>
public record CryptRequest(string Message);

/// <summary>
/// Response containing a protected message.
/// </summary>
public record CryptResponse(string Message);