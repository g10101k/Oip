using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Oip.Security.Api.Models.ApiResources;

public class ApiResourceApiDto
{
    public int Id { get; set; }

    [Required] public string Name { get; set; }

    public string DisplayName { get; set; }

    public string Description { get; set; }

    public bool Enabled { get; set; } = true;

    public bool ShowInDiscoveryDocument { get; set; }

    public List<string> UserClaims { get; set; } = new();

    public List<string> AllowedAccessTokenSigningAlgorithms { get; set; } = new();

    public List<string> Scopes { get; set; } = new();
}