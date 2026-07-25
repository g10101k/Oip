using System.ComponentModel.DataAnnotations;

namespace Oip.Demo.TableQueryDemo;

public class SaveDemoCustomerRequest
{
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    [Required]
    public DemoCustomerStatus? Status { get; set; }

    [Range(0, 1000)]
    public int CreditScore { get; set; }

    [Range(0, double.MaxValue)]
    public decimal LifetimeValue { get; set; }
}
