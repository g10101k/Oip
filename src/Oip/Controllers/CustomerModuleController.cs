using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oip.Base.Controllers;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Dtos;
using Oip.Base.Data.Repositories;
using Oip.Base.Data.Services;
using Oip.Base.Exceptions;
using Oip.Base.Security;
using Oip.Base.Security.Objects;
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
    ObjectAccessService<DemoCustomerTableContext> access,
    ModuleRepository moduleRepository)
    : BaseModuleController<CustomerModuleSettings>(moduleRepository)
{
    /// <inheritdoc />
    public override string? Icon => "pi pi-id-card";

    private const string ViewFinancialsRight = "view-financials";

    private static readonly string[] RowRights =
        [SecurityConstants.Edit, SecurityConstants.Delete, SecurityConstants.ManageAcl];

    /// <summary>
    /// Retrieves a filtered page of customers for the customer module table.
    /// </summary>
    [Right(SecurityConstants.Read), HttpPost("get-page")]
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
                .WhereAccessible(access, SecurityConstants.Read)
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

            if (!await HasInstanceRight(ViewFinancialsRight))
                result = result with { Data = result.Data.Select(MaskFinancials).ToList() };

            var rights = await access.GetRightsByIdsAsync<DemoCustomer>(
                result.Data.Select(x => (long)x.Id).ToList(), RowRights, cancellationToken);
            result = result with
            {
                Data = result.Data.Select(x => x with { Rights = rights.GetValueOrDefault(x.Id, []) }).ToList()
            };

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
    [Right(SecurityConstants.Read)]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string[]>> GetCategories(CancellationToken cancellationToken = default)
    {
        var categories = await context.Categories
            .AsNoTracking()
            .WhereAccessible(access, SecurityConstants.Read)
            .OrderBy(x => x.Name)
            .Select(x => x.Name)
            .ToArrayAsync(cancellationToken);

        return Ok(categories);
    }

    /// <summary>
    /// Retrieves the customer categories the user can read, with the rights of the user on each of them.
    /// </summary>
    [HttpGet("get-customer-categories")]
    [Right(SecurityConstants.Read)]
    [ProducesResponseType(typeof(List<DemoCustomerCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<DemoCustomerCategoryDto>>> GetCustomerCategories(
        CancellationToken cancellationToken = default)
    {
        var categories = await context.Categories
            .AsNoTracking()
            .WhereAccessible(access, SecurityConstants.Read)
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name })
            .ToListAsync(cancellationToken);

        var rights = await access.GetRightsByIdsAsync<DemoCustomerCategory>(
            categories.Select(x => (long)x.Id).ToList(), RowRights, cancellationToken);
        return Ok(categories
            .Select(x => new DemoCustomerCategoryDto(x.Id, x.Name, rights.GetValueOrDefault(x.Id, [])))
            .ToList());
    }

    /// <summary>
    /// Retrieves available customer countries.
    /// </summary>
    [HttpGet("get-countries")]
    [Right(SecurityConstants.Read)]
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
    [Right(SecurityConstants.Edit)]
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
        await DemandCategoryEditAsync(customer.Category, cancellationToken);

        context.Customers.Add(customer);
        await context.SaveChangesAsync(cancellationToken);
        await LoadRelationsAsync(customer, cancellationToken);

        return Ok(await MapCustomerWithRights(customer));
    }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    [HttpPut("update/{id}")]
    [Right(SecurityConstants.Edit)]
    [ObjectRight(DemoCustomerTableContext.CustomerObjectType, SecurityConstants.Edit)]
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
            throw new ApiException("Customer not found", $"Customer with id {id} was not found.",
                StatusCodes.Status404NotFound);

        var previousCategoryId = customer.CategoryId;
        await BuildCustomerAsync(customer, request, cancellationToken);
        if (customer.Category.Id != previousCategoryId)
            await DemandCategoryEditAsync(customer.Category, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await LoadRelationsAsync(customer, cancellationToken);

        return Ok(await MapCustomerWithRights(customer));
    }

    /// <summary>
    /// Deletes a customer.
    /// </summary>
    [HttpDelete("delete/{id}")]
    [Right(SecurityConstants.Delete)]
    [ObjectRight(DemoCustomerTableContext.CustomerObjectType, SecurityConstants.Delete)]
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
            throw new ApiException("Customer not found", $"Customer with id {id} was not found.",
                StatusCodes.Status404NotFound);

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

    private async Task<DemoCustomerTableRowDto> MapCustomerWithRights(DemoCustomer customer)
    {
        var dto = MapCustomer(customer) with
        {
            Rights = (await access.GetEffectiveRightsAsync(
                    new ObjectRef(DemoCustomerTableContext.CustomerObjectType, customer.Id)))
                .Where(RowRights.Contains)
                .ToList()
        };
        return await HasInstanceRight(ViewFinancialsRight) ? dto : MaskFinancials(dto);
    }

    /// <summary>
    /// A customer can be placed into an existing category only with the edit right on it. A new category gets the
    /// default rights of the type, so it needs no check.
    /// </summary>
    private async Task DemandCategoryEditAsync(DemoCustomerCategory category, CancellationToken cancellationToken)
    {
        if (category.Id == 0)
            return;

        var categoryRef = new ObjectRef(DemoCustomerTableContext.CategoryObjectType, category.Id);
        if (!await access.HasRightAsync(categoryRef, SecurityConstants.Edit, cancellationToken))
            throw new ApiException("Forbidden", $"The '{SecurityConstants.Edit}' right on category '{category.Name}' is required.",
                StatusCodes.Status403Forbidden);
    }

    private static DemoCustomerTableRowDto MaskFinancials(DemoCustomerTableRowDto row)
    {
        return row with { LifetimeValue = 0 };
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
            new()
            {
                Code = ViewFinancialsRight,
                Name = "View financials",
                Description = "Can view customer lifetime value",
                Roles = [SecurityConstants.AdminRole]
            }
        };
    }
}

/// <summary>
/// Module settings for the customer module.
/// </summary>
public class CustomerModuleSettings
{
}