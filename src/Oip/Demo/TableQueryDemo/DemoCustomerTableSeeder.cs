namespace Oip.Demo.TableQueryDemo;

public static class DemoCustomerTableSeeder
{
    public static void Seed(DemoCustomerTableContext context)
    {
        if (context.Customers.Any())
        {
            return;
        }

        var categories = new List<DemoCustomerCategory>
        {
            new() { Id = 1, Name = "Retail" },
            new() { Id = 2, Name = "Enterprise" },
            new() { Id = 3, Name = "Government" },
            new() { Id = 4, Name = "Startup" }
        };

        var countries = new List<DemoCountry>
        {
            new() { Id = 1, Name = "Germany" },
            new() { Id = 2, Name = "Canada" },
            new() { Id = 3, Name = "Japan" },
            new() { Id = 4, Name = "Brazil" },
            new() { Id = 5, Name = "Spain" }
        };

        context.Categories.AddRange(categories);
        context.Countries.AddRange(countries);

        var customers = new List<DemoCustomer>();
        var orders = new List<DemoOrder>();

        var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var statuses = Enum.GetValues<DemoCustomerStatus>();

        for (var i = 1; i <= 100; i++)
        {
            var category = categories[(i - 1) % categories.Count];
            var country = countries[(i - 1) % countries.Count];
            var firstName = $"Customer{i:000}";
            var lastName = $"Last{i:000}";

            var customer = new DemoCustomer
            {
                Id = i,
                FirstName = firstName,
                LastName = lastName,
                Email = $"customer{i:000}@example.com",
                CreatedAt = baseDate.AddDays(i),
                Status = statuses[(i - 1) % statuses.Length],
                IsActive = i % 5 != 0,
                CreditScore = 350 + (i * 5 % 501),
                LifetimeValue = Math.Round(500m + i * 123.45m + (i % 4) * 75.5m, 2),
                CategoryId = category.Id,
                Category = category,
                CountryId = country.Id,
                Country = country
            };

            customers.Add(customer);
            category.Customers.Add(customer);
            country.Customers.Add(customer);

            var orderCount = i % 4;
            for (var orderIndex = 1; orderIndex <= orderCount; orderIndex++)
            {
                orders.Add(new DemoOrder
                {
                    Id = (i - 1) * 4 + orderIndex,
                    CustomerId = customer.Id,
                    Customer = customer,
                    TotalAmount = 100 + i * 3 + orderIndex * 17,
                    CreatedAt = customer.CreatedAt.AddDays(orderIndex)
                });
            }
        }

        context.Customers.AddRange(customers);
        context.Orders.AddRange(orders);
        context.SaveChanges();
    }
}
