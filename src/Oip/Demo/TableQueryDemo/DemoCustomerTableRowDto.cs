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
    int OrdersCount);
