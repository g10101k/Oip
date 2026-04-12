namespace Oip.Demo.TableQueryDemo;

public class DemoCustomerCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<DemoCustomer> Customers { get; set; } = new();
}
