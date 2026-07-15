// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for PlantSafety's general-purpose
//   Reporting module. Wraps all HTTP communication for the Reports List and
//   Report Detail pages. All methods return ApiResponseEnvelope<T>.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.Reporting.Models;

namespace PlantSafety.UI.Modules.Reporting.ApiClients;

/// <summary>Typed API client for the Reporting module.</summary>
public sealed class ReportingApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="ReportingApiClient"/>.</summary>
    public ReportingApiClient(HttpClient http, ILogger<ReportingApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves a paginated page of reports.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<ReportListItemDto>>> GetReportsAsync(
        int pageNumber = 1, int pageSize = 25, string? typeFilter = null, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<ReportListItemDto>>(
            $"v1/reporting/reports?pageNumber={pageNumber}&pageSize={pageSize}" +
            (string.IsNullOrWhiteSpace(typeFilter) ? string.Empty : $"&reportType={Uri.EscapeDataString(typeFilter)}"),
            cancellationToken);

    /// <summary>Retrieves the full detail for a single report.</summary>
    public Task<ApiResponseEnvelope<ReportDetailDto>> GetReportAsync(Guid reportId, CancellationToken cancellationToken = default)
        => GetAsync<ReportDetailDto>($"v1/reporting/reports/{reportId}", cancellationToken);
}
