using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oip.Api.Controllers;
using Oip.Api.Controllers.Api;
using Oip.Base.Exceptions;
using Oip.Data.Constants;
using Oip.Data.Dtos;
using Oip.Data.Repositories;
using Oip.Data.Services;
using Oip.Demo.TableQueryDemo;
using Oip.Properties;

namespace Oip.Controllers;

/// <summary>
/// Module that exposes demo customers for PrimeNG p-table server-side paging and filtering.
/// </summary>
[ApiController]
[Authorize]
[Route("api/customer-module")]
public class CustomerModuleController(
    DemoCustomerTableContext context,
    ModuleRepository moduleRepository)
    : BaseModuleController<CustomerModuleSettings>(moduleRepository)
{
    /// <summary>
    /// Retrieves a filtered page of customers for the customer module table.
    /// </summary>
    [HttpPost("get-page")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TablePageResult<DemoCustomerTableRowDto>>> GetPage(
        [FromBody] TableQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new TableQueryOptions
            {
                FieldMappings =
                {
                    ["fullName"] = nameof(DemoCustomer.FirstName),
                    ["email"] = nameof(DemoCustomer.Email),
                    ["createdAt"] = nameof(DemoCustomer.CreatedAt),
                    ["status"] = nameof(DemoCustomer.Status),
                    ["isActive"] = nameof(DemoCustomer.IsActive),
                    ["creditScore"] = nameof(DemoCustomer.CreditScore),
                    ["lifetimeValue"] = nameof(DemoCustomer.LifetimeValue),
                    ["categoryName"] = $"{nameof(DemoCustomer.Category)}.{nameof(DemoCustomerCategory.Name)}",
                    ["countryName"] = $"{nameof(DemoCustomer.Country)}.{nameof(DemoCountry.Name)}"
                }
            };
            options.GlobalFilterFields.Add(nameof(DemoCustomer.FirstName));
            options.GlobalFilterFields.Add(nameof(DemoCustomer.LastName));
            options.GlobalFilterFields.Add(nameof(DemoCustomer.Email));
            options.GlobalFilterFields.Add($"{nameof(DemoCustomer.Category)}.{nameof(DemoCustomerCategory.Name)}");
            options.GlobalFilterFields.Add($"{nameof(DemoCustomer.Country)}.{nameof(DemoCountry.Name)}");
            options.MaxRows = int.MaxValue;

            var query = context.Customers
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Country)
                .Include(x => x.Orders);

            var result = await TableQueryProcessor.ExecuteAsync(
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
                options,
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            throw new ApiException("Invalid customer table query", ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    /// <summary>
    /// Retrieves available customer categories.
    /// </summary>
    [HttpGet("get-categories")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string[]>> GetCategories(CancellationToken cancellationToken = default)
    {
        var categories = await context.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => x.Name)
            .ToArrayAsync(cancellationToken);

        return Ok(categories);
    }

    /// <summary>
    /// Retrieves available customer countries.
    /// </summary>
    [HttpGet("get-countries")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string[]>> GetCountries(CancellationToken cancellationToken = default)
    {
        var countries = await context.Countries
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => x.Name)
            .ToArrayAsync(cancellationToken);

        return Ok(countries);
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    [HttpPost("create")]
    [ProducesResponseType(typeof(DemoCustomerTableRowDto), StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DemoCustomerTableRowDto>> Create(
        [FromBody] SaveDemoCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await BuildCustomerAsync(new DemoCustomer
        {
            CreatedAt = DateTime.UtcNow,
            IsActive = request.Status == DemoCustomerStatus.Active
        }, request, cancellationToken);

        context.Customers.Add(customer);
        await context.SaveChangesAsync(cancellationToken);
        await LoadRelationsAsync(customer, cancellationToken);

        return Ok(MapCustomer(customer));
    }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    [HttpPut("update/{id}")]
    [ProducesResponseType(typeof(DemoCustomerTableRowDto), StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DemoCustomerTableRowDto>> Update(
        int id,
        [FromBody] SaveDemoCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        var customer = await context.Customers
            .Include(x => x.Category)
            .Include(x => x.Country)
            .Include(x => x.Orders)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (customer is null)
            throw new ApiException("Customer not found", $"Customer with id {id} was not found.", StatusCodes.Status404NotFound);

        await BuildCustomerAsync(customer, request, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await LoadRelationsAsync(customer, cancellationToken);

        return Ok(MapCustomer(customer));
    }

    /// <summary>
    /// Deletes a customer.
    /// </summary>
    [HttpDelete("delete/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken = default)
    {
        var customer = await context.Customers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (customer is null)
            throw new ApiException("Customer not found", $"Customer with id {id} was not found.", StatusCodes.Status404NotFound);

        context.Customers.Remove(customer);
        await context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task<DemoCustomer> BuildCustomerAsync(
        DemoCustomer customer,
        SaveDemoCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var fullName = request.FullName.Trim();
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ApiException("Invalid customer data", "Full name is required.", StatusCodes.Status400BadRequest);

        var nameParts = fullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (nameParts.Length == 0)
            throw new ApiException("Invalid customer data", "Full name is required.", StatusCodes.Status400BadRequest);

        var categoryName = request.Category.Trim();
        var countryName = request.Country.Trim();
        var email = request.Email.Trim();

        if (string.IsNullOrWhiteSpace(categoryName))
            throw new ApiException("Invalid customer data", "Category is required.", StatusCodes.Status400BadRequest);

        if (string.IsNullOrWhiteSpace(countryName))
            throw new ApiException("Invalid customer data", "Country is required.", StatusCodes.Status400BadRequest);

        if (string.IsNullOrWhiteSpace(email))
            throw new ApiException("Invalid customer data", "Email is required.", StatusCodes.Status400BadRequest);

        if (request.Status is null)
            throw new ApiException("Invalid customer data", "Status is required.", StatusCodes.Status400BadRequest);

        var category = await context.Categories.FirstOrDefaultAsync(x => x.Name == categoryName, cancellationToken)
            ?? new DemoCustomerCategory { Name = categoryName };
        if (category.Id == 0)
            context.Categories.Add(category);

        var country = await context.Countries.FirstOrDefaultAsync(x => x.Name == countryName, cancellationToken)
            ?? new DemoCountry { Name = countryName };
        if (country.Id == 0)
            context.Countries.Add(country);

        customer.FirstName = nameParts[0];
        customer.LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;
        customer.Email = email;
        customer.Category = category;
        customer.Country = country;
        customer.Status = request.Status.Value;
        customer.IsActive = request.Status == DemoCustomerStatus.Active;
        customer.CreditScore = request.CreditScore;
        customer.LifetimeValue = request.LifetimeValue;

        return customer;
    }

    private async Task LoadRelationsAsync(DemoCustomer customer, CancellationToken cancellationToken)
    {
        await context.Entry(customer).Reference(x => x.Category).LoadAsync(cancellationToken);
        await context.Entry(customer).Reference(x => x.Country).LoadAsync(cancellationToken);
        await context.Entry(customer).Collection(x => x.Orders).LoadAsync(cancellationToken);
    }

    private static DemoCustomerTableRowDto MapCustomer(DemoCustomer customer)
    {
        var fullName = string.Join(" ", new[] { customer.FirstName, customer.LastName }
            .Where(x => !string.IsNullOrWhiteSpace(x)));

        return new DemoCustomerTableRowDto(
            customer.Id,
            fullName,
            customer.Email,
            customer.Category.Name,
            customer.Country.Name,
            customer.Status,
            customer.IsActive,
            customer.CreditScore,
            customer.LifetimeValue,
            customer.CreatedAt,
            customer.Orders.Count);
    }
    
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public override List<SecurityResponse> GetModuleRights()
    {
        return new()
        {
            new()
            {
                Code = SecurityConstants.Read,
                Name = Resources.CustomerModuleController_GetModuleRights_Read,
                Description = Resources.CustomerModuleController_GetModuleRights_Can_view_customer_module,
                Roles = [SecurityConstants.AdminRole]
            },
            new()
            {
                Code = SecurityConstants.Edit,
                Name = Resources.CustomerModuleController_GetModuleRights_Edit,
                Description = Resources.CustomerModuleController_GetModuleRights_Can_edit_customer_data,
                Roles = [SecurityConstants.AdminRole]
            },
            new()
            {
                Code = SecurityConstants.Delete,
                Name = Resources.CustomerModuleController_GetModuleRights_Delete,
                Description = Resources.CustomerModuleController_GetModuleRights_Can_delete_customer_data,
                Roles = [SecurityConstants.AdminRole]
            },
        };
    }
}

/// <summary>
/// Module settings for the customer module.
/// </summary>
public class CustomerModuleSettings
{
}
