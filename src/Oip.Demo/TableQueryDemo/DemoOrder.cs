namespace Oip.Demo.TableQueryDemo;

public class DemoOrder
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public DemoCustomer Customer { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; }
}
