using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Oip.Data.Dtos;
using Oip.Data.Services;
using Oip.Demo.TableQueryDemo;

namespace Oip.Test;

[TestFixture]
public class TableQueryProcessorTests
{
    [Test]
    public async Task GetPage_ReturnsFirstPage_WhenPagingWithoutFilters()
    {
        await using var context = CreateContext();

        var payload = await ExecuteRequest(context, new TableQueryRequest
        {
            First = 0,
            Rows = 10,
            SortField = "email",
            SortOrder = 1
        });

        Assert.Multiple(() =>
        {
            Assert.That(payload.Total, Is.EqualTo(100));
            Assert.That(payload.Rows, Is.EqualTo(10));
            Assert.That(payload.Data, Has.Count.EqualTo(10));
            Assert.That(payload.Data[0].Email, Is.EqualTo("customer001@example.com"));
        });
    }

    [Test]
    public async Task GetPage_AllowsLargePageSize_ForExtendedRowsPerPageOptions()
    {
        await using var context = CreateContext();

        var payload = await ExecuteRequest(context, new TableQueryRequest
        {
            First = 0,
            Rows = 500,
            SortField = "email",
            SortOrder = 1
        });

        Assert.Multiple(() =>
        {
            Assert.That(payload.Total, Is.EqualTo(100));
            Assert.That(payload.Rows, Is.EqualTo(500));
            Assert.That(payload.Data, Has.Count.EqualTo(100));
        });
    }

    [Test]
    public async Task GetPage_AllowsRowsValueGreaterThanDatasetSize_ForShowAllMode()
    {
        await using var context = CreateContext();

        var payload = await ExecuteRequest(context, new TableQueryRequest
        {
            First = 0,
            Rows = 5000,
            SortField = "email",
            SortOrder = 1
        });

        Assert.Multiple(() =>
        {
            Assert.That(payload.Total, Is.EqualTo(100));
            Assert.That(payload.Rows, Is.EqualTo(5000));
            Assert.That(payload.Data, Has.Count.EqualTo(100));
        });
    }

    [Test]
    public async Task GetPage_FiltersByNestedProperty_WhenCategoryNameIsProvided()
    {
        await using var context = CreateContext();

        var payload = await ExecuteRequest(context, new TableQueryRequest
        {
            First = 0,
            Rows = 50,
            Filters = new Dictionary<string, JsonElement>
            {
                ["categoryName"] = JsonSerializer.SerializeToElement(new
                {
                    value = "Retail",
                    matchMode = "equals"
                })
            }
        });

        Assert.Multiple(() =>
        {
            Assert.That(payload.Total, Is.EqualTo(25));
            Assert.That(payload.Data, Has.Count.EqualTo(25));
            Assert.That(payload.Data.All(x => x.Category == "Retail"), Is.True);
        });
    }

    [Test]
    public async Task GetPage_FiltersByNestedProperty_CaseInsensitiveStartsWith()
    {
        await using var context = CreateContext();

        var payload = await ExecuteRequest(context, new TableQueryRequest
        {
            First = 0,
            Rows = 50,
            Filters = new Dictionary<string, JsonElement>
            {
                ["categoryName"] = JsonSerializer.SerializeToElement(new[]
                {
                    new
                    {
                        value = "re",
                        matchMode = "startsWith",
                        @operator = "and"
                    }
                })
            },
            SortField = "categoryName",
            SortOrder = 1
        });

        Assert.Multiple(() =>
        {
            Assert.That(payload.Total, Is.EqualTo(25));
            Assert.That(payload.Data, Has.Count.EqualTo(25));
            Assert.That(payload.Data.All(x => x.Category == "Retail"), Is.True);
        });
    }

    [Test]
    public async Task GetPage_AcceptsPrimeNgArrayFilterPayload()
    {
        await using var context = CreateContext();

        var payload = await ExecuteRequest(context, new TableQueryRequest
        {
            First = 0,
            Rows = 20,
            Filters = new Dictionary<string, JsonElement>
            {
                ["fullName"] = JsonSerializer.SerializeToElement(new[]
                {
                    new
                    {
                        value = "Customer01",
                        matchMode = "contains"
                    }
                })
            }
        });

        Assert.Multiple(() =>
        {
            Assert.That(payload.Total, Is.EqualTo(10));
            Assert.That(payload.Data, Has.Count.EqualTo(10));
            Assert.That(payload.Data.All(x => x.FullName!.Contains("Customer01", StringComparison.Ordinal)), Is.True);
        });
    }

    [Test]
    public async Task GetPage_AppliesGlobalFilterAcrossConfiguredFields()
    {
        await using var context = CreateContext();

        var payload = await ExecuteRequest(context, new TableQueryRequest
        {
            First = 0,
            Rows = 20,
            GlobalFilter = "Japan"
        });

        Assert.Multiple(() =>
        {
            Assert.That(payload.Total, Is.EqualTo(20));
            Assert.That(payload.Data, Has.Count.EqualTo(20));
            Assert.That(payload.Data.All(x => x.Country == "Japan"), Is.True);
        });
    }

    [Test]
    public async Task GetPage_SortsDescendingByDate()
    {
        await using var context = CreateContext();

        var payload = await ExecuteRequest(context, new TableQueryRequest
        {
            First = 0,
            Rows = 5,
            SortField = "createdAt",
            SortOrder = -1
        });

        Assert.That(payload.Data.Select(x => x.Id).ToArray(), Is.EqualTo(new[] { 100, 99, 98, 97, 96 }));
    }

    [Test]
    public async Task GetPage_SupportsDateIsMatchMode()
    {
        await using var context = CreateContext();

        var payload = await ExecuteRequest(context, new TableQueryRequest
        {
            First = 0,
            Rows = 20,
            Filters = new Dictionary<string, JsonElement>
            {
                ["createdAt"] = JsonSerializer.SerializeToElement(new
                {
                    value = "2024-01-12T00:00:00.000Z",
                    matchMode = "dateIs"
                })
            }
        });

        Assert.Multiple(() =>
        {
            Assert.That(payload.Total, Is.EqualTo(1));
            Assert.That(payload.Data, Has.Count.EqualTo(1));
            Assert.That(payload.Data[0].Id, Is.EqualTo(11));
        });
    }

    [Test]
    public async Task GetPage_FiltersNumericField_WithGreaterThan()
    {
        await using var context = CreateContext();

        var payload = await ExecuteRequest(context, new TableQueryRequest
        {
            First = 0,
            Rows = 100,
            Filters = new Dictionary<string, JsonElement>
            {
                ["creditScore"] = JsonSerializer.SerializeToElement(new
                {
                    value = 800,
                    matchMode = "gt"
                })
            }
        });

        Assert.Multiple(() =>
        {
            Assert.That(payload.Total, Is.EqualTo(10));
            Assert.That(payload.Data, Has.Count.EqualTo(10));
            Assert.That(payload.Data.All(x => x.CreditScore > 800), Is.True);
        });
    }

    [Test]
    public async Task GetPage_FiltersNumericField_WithLessThanOrEqual()
    {
        await using var context = CreateContext();

        var payload = await ExecuteRequest(context, new TableQueryRequest
        {
            First = 0,
            Rows = 100,
            Filters = new Dictionary<string, JsonElement>
            {
                ["lifetimeValue"] = JsonSerializer.SerializeToElement(new
                {
                    value = 1000,
                    matchMode = "lte"
                })
            }
        });

        Assert.Multiple(() =>
        {
            Assert.That(payload.Total, Is.EqualTo(3));
            Assert.That(payload.Data, Has.Count.EqualTo(3));
            Assert.That(payload.Data.All(x => x.LifetimeValue <= 1000m), Is.True);
        });
    }

    [Test]
    public async Task GetPage_CanCombineNumericAndTextFilters()
    {
        await using var context = CreateContext();

        var payload = await ExecuteRequest(context, new TableQueryRequest
        {
            First = 0,
            Rows = 100,
            Filters = new Dictionary<string, JsonElement>
            {
                ["categoryName"] = JsonSerializer.SerializeToElement(new
                {
                    value = "Retail",
                    matchMode = "equals"
                }),
                ["creditScore"] = JsonSerializer.SerializeToElement(new
                {
                    value = 700,
                    matchMode = "gte"
                })
            }
        });

        Assert.Multiple(() =>
        {
            Assert.That(payload.Total, Is.EqualTo(7));
            Assert.That(payload.Data, Has.Count.EqualTo(7));
            Assert.That(payload.Data.All(x => x.Category == "Retail" && x.CreditScore >= 700), Is.True);
        });
    }

    [Test]
    public async Task GetPage_FiltersEnumField_WithCaseInsensitiveStartsWith()
    {
        await using var context = CreateContext();

        var payload = await ExecuteRequest(context, new TableQueryRequest
        {
            First = 0,
            Rows = 100,
            Filters = new Dictionary<string, JsonElement>
            {
                ["status"] = JsonSerializer.SerializeToElement(new[]
                {
                    new
                    {
                        value = "act",
                        matchMode = "startsWith",
                        @operator = "and"
                    }
                })
            }
        });

        Assert.Multiple(() =>
        {
            Assert.That(payload.Total, Is.EqualTo(33));
            Assert.That(payload.Data, Has.Count.EqualTo(33));
            Assert.That(payload.Data.All(x => x.Status == DemoCustomerStatus.Active), Is.True);
        });
    }

    [Test]
    public void GetPage_ThrowsArgumentException_ForUnsupportedField()
    {
        using var context = CreateContext();

        var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            await ExecuteRequest(context, new TableQueryRequest
            {
                First = 0,
                Rows = 10,
                SortField = "unknownField"
            }));

        Assert.That(exception, Is.Not.Null);
    }

    private static Task<TablePageResult<DemoCustomerTableRowDto>> ExecuteRequest(
        DemoCustomerTableContext context,
        TableQueryRequest request)
    {
        var query = context.Customers
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Country)
            .Include(x => x.Orders);

        var options = new TableQueryOptions();
        options.FieldMappings["fullName"] = nameof(DemoCustomer.FirstName);
        options.FieldMappings["email"] = nameof(DemoCustomer.Email);
        options.FieldMappings["createdAt"] = nameof(DemoCustomer.CreatedAt);
        options.FieldMappings["status"] = nameof(DemoCustomer.Status);
        options.FieldMappings["isActive"] = nameof(DemoCustomer.IsActive);
        options.FieldMappings["creditScore"] = nameof(DemoCustomer.CreditScore);
        options.FieldMappings["lifetimeValue"] = nameof(DemoCustomer.LifetimeValue);
        options.FieldMappings["categoryName"] = $"{nameof(DemoCustomer.Category)}.{nameof(DemoCustomerCategory.Name)}";
        options.FieldMappings["countryName"] = $"{nameof(DemoCustomer.Country)}.{nameof(DemoCountry.Name)}";
        options.GlobalFilterFields.Add(nameof(DemoCustomer.FirstName));
        options.GlobalFilterFields.Add(nameof(DemoCustomer.LastName));
        options.GlobalFilterFields.Add(nameof(DemoCustomer.Email));
        options.GlobalFilterFields.Add($"{nameof(DemoCustomer.Category)}.{nameof(DemoCustomerCategory.Name)}");
        options.GlobalFilterFields.Add($"{nameof(DemoCustomer.Country)}.{nameof(DemoCountry.Name)}");
        options.MaxRows = int.MaxValue;

        return TableQueryProcessor.ExecuteAsync(
            query,
            request,
            customer => new DemoCustomerTableRowDto(
                customer.Id,
                customer.FirstName + " " + customer.LastName,
                customer.Email,
                customer.Category.Name,
                customer.Country.Name,
                customer.Status,
                customer.IsActive,
                customer.CreditScore,
                customer.LifetimeValue,
                customer.CreatedAt,
                customer.Orders.Count),
            options);
    }

    private static DemoCustomerTableContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DemoCustomerTableContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new DemoCustomerTableContext(options);
        DemoCustomerTableSeeder.Seed(context);
        return context;
    }
}
