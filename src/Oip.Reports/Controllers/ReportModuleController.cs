using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oip.Base.Controllers;
using Oip.Base.Controllers.Api;
using Oip.Base.Data.Constants;
using Oip.Base.Data.Repositories;
using Oip.Base.Exceptions;
using Oip.Reports.Base.Abstractions;
using Oip.Reports.Base.Models;

namespace Oip.Reports.Controllers;

[ApiController]
[Authorize]
[Route("api/report-module")]
public class ReportModuleController(
    ModuleRepository moduleRepository,
    IReportDefinitionService reportDefinitionService,
    IReportGenerationService reportGenerationService,
    IReportExportService reportExportService)
    : BaseModuleController<ReportModuleSettings>(moduleRepository)
{
    [HttpGet("get-reports")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ReportDefinitionSummary>), StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyCollection<ReportDefinitionSummary>>> GetReports(CancellationToken cancellationToken = default)
    {
        return Ok(await reportDefinitionService.GetReportsAsync(cancellationToken));
    }

    [HttpPost("create-report")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType(typeof(ReportDefinition), StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReportDefinition>> CreateReport([FromBody] ReportDefinition definition, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(definition.Name))
            throw new ApiException("Invalid report definition", "Report name is required.", StatusCodes.Status400BadRequest);

        return Ok(await reportDefinitionService.CreateReportAsync(definition, cancellationToken));
    }

    [HttpPost("get-report-preview")]
    [ProducesResponseType(typeof(ReportResult), StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReportResult>> GetReportPreview([FromBody] ReportRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            request.Format = ReportExportFormat.Html;
            return Ok(await reportGenerationService.GetPreviewAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            throw new ApiException("Report preview failed", ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    [HttpPost("get-report-export")]
    [ProducesResponseType(typeof(ReportExportResult), StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReportExportResult>> GetReportExport([FromBody] ReportRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await reportExportService.ExportAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            throw new ApiException("Report export failed", ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    [HttpGet("get-report-by-id")]
    [ProducesResponseType(typeof(ReportDefinition), StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReportDefinition>> GetReportById([FromQuery] string id, CancellationToken cancellationToken = default)
    {
        var report = await reportDefinitionService.GetReportByIdAsync(id, cancellationToken);
        if (report is null)
            throw new ApiException("Report not found", $"Report with id '{id}' was not found.", StatusCodes.Status404NotFound);

        return Ok(report);
    }

    [HttpGet("get-report-data-source-schema-by-report-id")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType(typeof(IReadOnlyCollection<ReportDataSourceSchema>), StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<ReportDataSourceSchema>>> GetReportDataSourceSchemaByReportId(
        [FromQuery] string id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await reportDefinitionService.GetDataSourceSchemaAsync(id, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            throw new ApiException("Report not found", ex.Message, StatusCodes.Status404NotFound);
        }
    }

    [HttpPut("update-report/{id}")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType(typeof(ReportDefinition), StatusCodes.Status200OK)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReportDefinition>> UpdateReport(string id, [FromBody] ReportDefinition definition, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(definition.Name))
            throw new ApiException("Invalid report definition", "Report name is required.", StatusCodes.Status400BadRequest);

        return Ok(await reportDefinitionService.UpdateReportAsync(id, definition, cancellationToken));
    }

    [HttpDelete("delete-report/{id}")]
    [Authorize(Roles = SecurityConstants.AdminRole)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ApiExceptionResponse>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteReport(string id, CancellationToken cancellationToken = default)
    {
        await reportDefinitionService.DeleteReportAsync(id, cancellationToken);
        return NoContent();
    }

    public override List<SecurityResponse> GetModuleRights()
    {
        return new()
        {
            new()
            {
                Code = SecurityConstants.Read,
                Name = "Read reports",
                Description = "Can open the report module and run previews.",
                Roles = [SecurityConstants.AdminRole]
            },
            new()
            {
                Code = SecurityConstants.Edit,
                Name = "Manage reports",
                Description = "Can create, update and delete report definitions.",
                Roles = [SecurityConstants.AdminRole]
            }
        };
    }
}

public class ReportModuleSettings
{
    public string DefaultReportId { get; set; } = "customer-directory";
}
