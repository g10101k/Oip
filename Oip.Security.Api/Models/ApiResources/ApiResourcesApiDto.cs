using System.Collections.Generic;

namespace Oip.Security.Api.Models.ApiResources;

public class ApiResourcesApiDto
{
    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public List<ApiResourceApiDto> ApiResources { get; set; } = new();
}