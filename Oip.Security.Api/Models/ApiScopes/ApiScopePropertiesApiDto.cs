using System.Collections.Generic;

namespace Oip.Security.Api.Dtos.ApiScopes;

public class ApiScopePropertiesApiDto
{
    public List<ApiScopePropertyApiDto> ApiScopeProperties { get; set; } = new();

    public int TotalCount { get; set; }

    public int PageSize { get; set; }
}