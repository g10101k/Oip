using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Oip.Base.Extensions;

namespace Oip.Base.Services;

internal sealed class CookieAuthenticationTicketStorePostConfigure(ITicketStore ticketStore)
    : IPostConfigureOptions<CookieAuthenticationOptions>
{
    public void PostConfigure(string? name, CookieAuthenticationOptions options)
    {
        if (name == OipModuleApplication.CookieAuthenticationScheme)
            options.SessionStore = ticketStore;
    }
}
