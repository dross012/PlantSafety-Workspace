// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Typed API client for the Alerts module. Wraps all
//   HTTP communication for alerts CRUD and lifecycle operations.
//   All methods return ApiResponseEnvelope<T>. No HttpClient calls
//   are permitted outside this class for the Alerts module.
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
using PlantSafety.UI.Modules.Alerts.Models;

namespace PlantSafety.UI.Modules.Alerts.ApiClients;

/// <summary>
/// Typed API client for the Alerts module.
/// Components MUST inject this class and call its methods rather than
/// constructing HTTP requests directly.
/// </summary>
public sealed class AlertsApiClient : SafetyApiClientBase
{
    /// <summary>
    /// Initializes a new instance of <see cref="AlertsApiClient"/>.
    /// </summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public AlertsApiClient(HttpClient http, ILogger<AlertsApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>
    /// Retrieves a paginated list of alerts for the authenticated tenant.
    /// </summary>
    /// <param name="pageNumber">One-based page number.</param>
    /// <param name="pageSize">Maximum items per page (API enforces max).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>An envelope containing the paginated alert list or a problem/fault.</returns>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<AlertListItemDto>>> GetAlertsAsync(
        int pageNumber = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<AlertListItemDto>>(
            $"v1/alerts?pageNumber={pageNumber}&pageSize={pageSize}",
            cancellationToken);

    /// <summary>
    /// Retrieves the full detail for a single alert by its identifier.
    /// </summary>
    /// <param name="alertId">The unique alert identifier.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>An envelope containing the full <see cref="AlertDto"/> or a problem/fault.</returns>
    public Task<ApiResponseEnvelope<AlertDto>> GetAlertAsync(
        Guid alertId,
        CancellationToken cancellationToken = default)
        => GetAsync<AlertDto>($"v1/alerts/{alertId}", cancellationToken);

    /// <summary>
    /// Acknowledges an alert, transitioning it to the Acknowledged status.
    /// </summary>
    /// <param name="alertId">The identifier of the alert to acknowledge.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>An envelope containing the updated <see cref="AlertDto"/> or a problem/fault.</returns>
    public Task<ApiResponseEnvelope<AlertDto>> AcknowledgeAlertAsync(
        Guid alertId,
        CancellationToken cancellationToken = default)
        => PostAsync<object, AlertDto>(
            $"v1/alerts/{alertId}/acknowledge",
            new { },
            cancellationToken);
}
