// ============================================================================
// File: OperationsAuditViewerV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-20
// Description: V1 controller surfacing a read-only search/export/diff view over the
//   shared, cross-database [PlantSafety.AuditDB].dbo.AuditLog table -- real platform
//   audit infrastructure written to by hundreds of stored procedures across all 8
//   module databases via the dbo.AuditLog synonym (Phase8-Structural\83-synonyms-audit-
//   error.sql). Routes all requests through MediatR ISender to the Application layer.
//   No domain logic or data access here.
//
//   SAFETY NOTE: This controller is deliberately READ-ONLY. There is no endpoint here
//   that writes to, purges, or otherwise mutates AuditLog -- it is an append-only audit
//   trail. Retention/purge of AuditLog is owned by Operations.Archival
//   (sp_AuditLog_Purge), not this module.
// ============================================================================
using System.Text;

using PlantSafety.Application.Operations.AuditViewer.Models;
using PlantSafety.Application.Operations.AuditViewer.Queries;

namespace PlantSafety.Api.Operations.AuditViewer.Controllers.V1;

/// <summary>V1 controller for the Operations.AuditViewer module.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/operations/audit-viewer")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Operations.AuditViewer")]
public sealed class OperationsAuditViewerV1Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="OperationsAuditViewerV1Controller"/>.</summary>
    public OperationsAuditViewerV1Controller(ISender sender) => _sender = sender;

    private string CorrelationId =>
        HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString() ?? string.Empty;

    /// <summary>Searches the shared AuditLog table by optional filters, newest first, paginated.</summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<AuditLogSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuditLogSearchResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchAsync(
        [FromQuery] string? tableName,
        [FromQuery] string? operation,
        [FromQuery] string? changedBy,
        [FromQuery] string? module,
        [FromQuery] int? severity,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] string? searchText,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var criteria = new AuditLogSearchCriteria(
            tableName, operation, changedBy, module, severity, fromUtc, toUtc, searchText, pageNumber, pageSize);

        var result = await _sender.Send(new SearchAuditLogQuery(criteria), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<AuditLogSearchResultDto>.Fail(result.Error.Message, CorrelationId));

        return Ok(ApiResponse<AuditLogSearchResultDto>.Ok(result.Value, CorrelationId));
    }

    /// <summary>
    /// Exports AuditLog rows matching the given filters as a flat CSV file, built server-side
    /// from the same search query (capped at 5000 rows per the underlying repository's
    /// MaxPageSize). Read-only -- never mutates AuditLog.
    /// </summary>
    [HttpGet("export")]
    [Produces("text/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuditLogSearchResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportAsync(
        [FromQuery] string? tableName,
        [FromQuery] string? operation,
        [FromQuery] string? changedBy,
        [FromQuery] string? module,
        [FromQuery] int? severity,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] string? searchText,
        CancellationToken cancellationToken = default)
    {
        // Export intentionally requests the maximum single page (no further pagination) --
        // the underlying repository caps this at AuditViewerOptions.MaxPageSize regardless.
        var criteria = new AuditLogSearchCriteria(
            tableName, operation, changedBy, module, severity, fromUtc, toUtc, searchText, PageNumber: 1, PageSize: 5000);

        var result = await _sender.Send(new SearchAuditLogQuery(criteria), cancellationToken);

        if (result.IsFailure)
            return BadRequest(ApiResponse<AuditLogSearchResultDto>.Fail(result.Error.Message, CorrelationId));

        var csvBytes = Encoding.UTF8.GetBytes(BuildCsv(result.Value.Items));
        var fileName = $"audit-log-export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        return File(csvBytes, "text/csv", fileName);
    }

    /// <summary>Builds a flat, RFC-4180-quoted CSV from the given audit log entries. No external library required.</summary>
    private static string BuildCsv(IReadOnlyList<AuditLogEntryDto> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine("TableName,SourceDatabase,Operation,ChangedBy,ChangeTime,ViewerAccessed,Severity,Module,ErrorContext,BeforeSnapshot,AfterSnapshot");

        foreach (var item in items)
        {
            sb.Append(CsvField(item.TableName)).Append(',');
            sb.Append(CsvField(item.SourceDatabase)).Append(',');
            sb.Append(CsvField(item.Operation)).Append(',');
            sb.Append(CsvField(item.ChangedBy)).Append(',');
            sb.Append(CsvField(item.ChangeTime.ToString("O"))).Append(',');
            sb.Append(CsvField(item.ViewerAccessed.ToString())).Append(',');
            sb.Append(CsvField(item.Severity.ToString())).Append(',');
            sb.Append(CsvField(item.Module)).Append(',');
            sb.Append(CsvField(item.ErrorContext)).Append(',');
            sb.Append(CsvField(item.BeforeSnapshot)).Append(',');
            sb.Append(CsvField(item.AfterSnapshot));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>Quotes and escapes a single CSV field per RFC 4180.</summary>
    private static string CsvField(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
