using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Oip.Security.Bl.Dtos.Configuration;

public class ApiResourcePropertiesDto
{
    public int ApiResourcePropertyId { get; set; }

    public int ApiResourceId { get; set; }

    public string ApiResourceName { get; set; }

    [Required] public string Key { get; set; }

    [Required] public string Value { get; set; }


    public int TotalCount { get; set; }

    public int PageSize { get; set; }

    public List<ApiResourcePropertyDto> ApiResourceProperties { get; set; } = new();
}