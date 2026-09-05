namespace Oip.Demo.TableQueryDemo;

/// <summary>
/// Represents a country entity within the demo system.
/// </summary>
public class DemoCountry
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<DemoCustomer> Customers { get; set; } = new();
}
