// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-20
// FILE DESCRIPTION: Typed API client for the Operations.AuditViewer module. Wraps HTTP
//   communication for PlantSafety.Api.Operations.AuditViewer.Controllers.V1
//   .OperationsAuditViewerV1Controller. Read-only: exposes search and CSV export only,
//   never a write/mutate call against AuditLog.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.OperationsAuditViewer.Models;

namespace PlantSafety.UI.Modules.OperationsAuditViewer.ApiClients;

/// <summary>Typed API client for the Operations.AuditViewer module.</summary>
public sealed class AuditViewerApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="AuditViewerApiClient"/>.</summary>
    public AuditViewerApiClient(HttpClient http, ILogger<AuditViewerApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Searches the shared AuditLog table by optional filters, newest first, paginated.</summary>
    public Task<ApiResponseEnvelope<AuditLogSearchResultDto>> SearchAsync(
        AuditLogFilter filter,
        CancellationToken cancellationToken = default)
        => GetAsync<AuditLogSearchResultDto>(BuildQueryString("v1/operations/audit-viewer/search", filter), cancellationToken);

    /// <summary>
    /// Downloads a CSV export of AuditLog rows matching the given filters, built server-side
    /// from the same search query. Returns the raw CSV bytes for the caller to save/trigger a
    /// browser download for -- this client never writes to disk itself.
    /// </summary>
    public async Task<byte[]?> DownloadExportCsvAsync(AuditLogFilter filter, CancellationToken cancellationToken = default)
    {
        var response = await Http.GetAsync(BuildQueryString("v1/operations/audit-viewer/export", filter), cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string BuildQueryString(string relativePath, AuditLogFilter filter)
    {
        var sb = new StringBuilder(relativePath);
        sb.Append('?');

        AppendParam(sb, "tableName", filter.TableName);
        AppendParam(sb, "operation", filter.Operation);
        AppendParam(sb, "changedBy", filter.ChangedBy);
        AppendParam(sb, "module", filter.Module);
        AppendParam(sb, "severity", filter.Severity?.ToString());
        AppendParam(sb, "fromUtc", filter.FromUtc?.ToString("o"));
        AppendParam(sb, "toUtc", filter.ToUtc?.ToString("o"));
        AppendParam(sb, "searchText", filter.SearchText);
        AppendParam(sb, "pageNumber", filter.PageNumber.ToString());
        AppendParam(sb, "pageSize", filter.PageSize.ToString());

        return sb.ToString().TrimEnd('&', '?');
    }

    private static void AppendParam(StringBuilder sb, string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        sb.Append(name).Append('=').Append(Uri.EscapeDataString(value)).Append('&');
    }
}
