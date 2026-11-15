using Oip.Base.Security.Objects;

namespace Oip.Demo.TableQueryDemo;

public class DemoCustomerCategory : ISecurableEntity
{
    public int Id { get; set; }

    public long AclSetId { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<DemoCustomer> Customers { get; set; } = new();
}
