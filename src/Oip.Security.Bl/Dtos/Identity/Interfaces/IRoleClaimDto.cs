﻿namespace Oip.Security.Bl.Dtos.Identity.Interfaces;

public interface IRoleClaimDto : IBaseRoleClaimDto
{
    string ClaimType { get; set; }
    string ClaimValue { get; set; }
}