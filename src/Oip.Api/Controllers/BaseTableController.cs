using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Exceptions;
using Oip.Data.Dtos;
using Oip.Data.Services;

namespace Oip.Api.Controllers;

/// <summary>
/// Base controller for PrimeNG p-table server-side pagination, sorting, and filtering.
/// </summary>
public abstract class BaseTableController2<TEntity, TRowDto> : ControllerBase
    where TEntity : class
{
    /// <summary>
    /// Returns the queryable source used by the table endpoint.
    /// </summary>
    protected abstract IQueryable<TEntity> BuildQuery();

    /// <summary>
    /// Returns the projection used for rows in the table result.
    /// </summary>
    protected abstract Expression<Func<TEntity, TRowDto>> BuildSelector();

    /// <summary>
    /// Returns the query options used to validate and map incoming table fields.
    /// </summary>
    protected virtual TableQueryOptions<TEntity> BuildOptions()
    {
        return new TableQueryOptions<TEntity>();
    }

    /// <summary>
    /// Retrieves a filtered page of rows for PrimeNG p-table lazy loading.
    /// </summary>
    [HttpPost("get-page")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public virtual async Task<ActionResult<TablePageResult<TRowDto>>> GetPage(
        [FromBody] TableQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await TableQueryProcessor.ExecuteAsync(
                BuildQuery(),
                request,
                BuildSelector(),
                BuildOptions(),
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            throw new ApiException("Invalid table query", ex.Message, StatusCodes.Status400BadRequest);
        }
    }
}
