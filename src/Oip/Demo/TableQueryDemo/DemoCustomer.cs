using Oip.Base.Security.Objects;

namespace Oip.Demo.TableQueryDemo;

public class DemoCustomer : ISecurableEntity
{
    public int Id { get; set; }

    public long AclSetId { get; set; }

    public string? OwnerSubject { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DemoCustomerStatus Status { get; set; }

    public bool IsActive { get; set; }

    public int CreditScore { get; set; }

    public decimal LifetimeValue { get; set; }

    public int CategoryId { get; set; }

    public DemoCustomerCategory Category { get; set; } = null!;

    public int CountryId { get; set; }

    public DemoCountry Country { get; set; } = null!;

    public List<DemoOrder> Orders { get; set; } = new();
}
