﻿using System.Collections.Generic;

namespace Oip.Security.STS.Identity.IntegrationTests.Common;

public static class RoutesConstants
{
    public static List<string> GetManageRoutes()
    {
        var manageRoutes = new List<string>
        {
            "Index",
            "ChangePassword",
            "PersonalData",
            "DeletePersonalData",
            "ExternalLogins",
            "TwoFactorAuthentication",
            "ResetAuthenticatorWarning",
            "EnableAuthenticator"
        };

        return manageRoutes;
    }
}