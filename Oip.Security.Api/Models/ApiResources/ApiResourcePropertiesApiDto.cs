using System.Collections.Generic;
using Oip.Security.Api.Dtos.ApiResources;

namespace Oip.Security.Api.Models.ApiResources;

public class ApiResourcePropertiesApiDto
{
    public List<ApiResourcePropertyApiDto> ApiResourceProperties { get; set; } = new();

    public int TotalCount { get; set; }

    public int PageSize { get; set; }
}