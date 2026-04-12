using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oip.Api.Controllers;
using Oip.Base.Exceptions;
using Oip.Data.Dtos;
using Oip.Data.Repositories;
using Oip.Data.Services;
using Oip.Demo.TableQueryDemo;

namespace Oip.Controllers;

/// <summary>
/// Module that exposes demo customers for PrimeNG p-table server-side paging and filtering.
/// </summary>
[ApiController]
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
    [Authorize]
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
            options.MaxRows = 100;

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
}

/// <summary>
/// Module settings for the customer module.
/// </summary>
public class CustomerModuleSettings
{
}
