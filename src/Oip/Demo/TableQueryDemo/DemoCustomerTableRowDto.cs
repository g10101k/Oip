namespace Oip.Demo.TableQueryDemo;

public record DemoCustomerTableRowDto(
    int Id,
    string FullName,
    string Email,
    string Category,
    string Country,
    DemoCustomerStatus Status,
    bool IsActive,
    int CreditScore,
    decimal LifetimeValue,
    DateTime CreatedAt,
    int OrdersCount)
{
    /// <summary>
    /// Rights of the current user on the customer besides read: edit, delete, manage-acl.
    /// </summary>
    public List<string> Rights { get; init; } = [];
}
