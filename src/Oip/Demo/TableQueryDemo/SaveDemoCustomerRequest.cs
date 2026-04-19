using System.ComponentModel.DataAnnotations;

namespace Oip.Demo.TableQueryDemo;

/// <summary>
/// Request for creating or updating a demo customer.
/// </summary>
public class SaveDemoCustomerRequest
{
    /// <summary>
    /// Full customer name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Customer email.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Customer category name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Customer country name.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Customer status.
    /// </summary>
    [Required]
    public DemoCustomerStatus? Status { get; set; }

    /// <summary>
    /// Credit score.
    /// </summary>
    [Range(0, 1000)]
    public int CreditScore { get; set; }

    /// <summary>
    /// Lifetime value.
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal LifetimeValue { get; set; }
}
