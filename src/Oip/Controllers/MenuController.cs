using Microsoft.AspNetCore.Mvc;

namespace Oip.Controllers;

/// <summary>
/// Menu controller
/// </summary>
[ApiController]
[Route("api/menu")]
public class MenuController : Controller
{
    /// <summary>
    /// Get menu for client app
    /// </summary>
    /// <returns></returns>
    [HttpGet("get")]
    public List<MenuDto> Get()
    {
        return
        [
            new MenuDto()
            {
                Label = "Home",
                Items =
                [
                    new MenuDto
                    {
                        Label = "Dashboard",
                        RouterLink = ["/dashboard/1"],
                        Icon = "pi pi-fw pi-home"
                    },
                    new MenuDto
                    {
                        Label = "Dashboard",
                        RouterLink = ["/dashboard/2"],
                        Icon = "pi pi-fw pi-home"
                    },
                    new MenuDto
                    {
                        Label = "Weather Forecast",
                        RouterLink = ["/weather/1"],
                        Icon = "pi pi-fw pi-sun"
                    }
                ]
            }
        ];
    }
}

/// <summary>
/// Menu item dto
/// </summary>
public class MenuDto
{
    /// <summary>
    /// Identification
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Label
    /// </summary>
    public string Label { get; set; } = null!;

    /// <summary>
    /// Icon
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Router Link
    /// </summary>
    public List<string>? RouterLink { get; set; }
    
    /// <summary>
    /// Url
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Target
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// Sub menu items
    /// </summary>
    public List<MenuDto>? Items { get; set; }
}