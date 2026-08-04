// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Typed API client for the LogisticsTransportSafety module. Wraps HTTP
//   communication with the real PlantSafety.Api endpoints under
//   api/v1/logisticstransportsafety/... (LogisticsTransportSafetyV1Controller). Calls gracefully
//   fault through ApiResponseEnvelope so the UI degrades to its Error state on backend failure.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using PlantSafety.UI.Infrastructure.ApiClients;
using PlantSafety.UI.Models.Common;
using PlantSafety.UI.Modules.LogisticsTransportSafety.Models;

namespace PlantSafety.UI.Modules.LogisticsTransportSafety.ApiClients;

/// <summary>Typed API client for the LogisticsTransportSafety module.</summary>
public sealed class LogisticsTransportSafetyApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="LogisticsTransportSafetyApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public LogisticsTransportSafetyApiClient(HttpClient http, ILogger<LogisticsTransportSafetyApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves carrier read-model data for a tenant/site within a date range, feeding the Fleet Safety Status dashboard.</summary>
    public Task<ApiResponseEnvelope<CarrierQueryResponseDto>> GetCarriersAsync(
        Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => GetAsync<CarrierQueryResponseDto>($"v1/logisticstransportsafety/carriers{BuildDateRangeQuery(siteId, from, to)}", cancellationToken);

    /// <summary>Retrieves all active (Planned or InProgress) transport trips, feeding the Active Trip Tracker dashboard.</summary>
    public Task<ApiResponseEnvelope<TransportTripQueryResponseDto>> ListActiveTripsAsync(
        Guid? siteId, CancellationToken cancellationToken = default)
    {
        var query = siteId.HasValue ? $"?SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}" : string.Empty;
        return GetAsync<TransportTripQueryResponseDto>($"v1/logisticstransportsafety/transport-trips/active{query}", cancellationToken);
    }

    /// <summary>Retrieves transport incident history for a tenant/site within a date range, feeding the Transport Incident Trends dashboard.</summary>
    public Task<ApiResponseEnvelope<TransportIncidentQueryResponseDto>> GetTransportIncidentHistoryAsync(
        Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => GetAsync<TransportIncidentQueryResponseDto>($"v1/logisticstransportsafety/transport-incidents{BuildDateRangeQuery(siteId, from, to)}", cancellationToken);

    private static string BuildDateRangeQuery(Guid? siteId, DateTime from, DateTime to)
    {
        var query = $"?From={Uri.EscapeDataString(from.ToString("O", CultureInfo.InvariantCulture))}" +
                    $"&To={Uri.EscapeDataString(to.ToString("O", CultureInfo.InvariantCulture))}";
        if (siteId.HasValue)
            query += $"&SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";
        return query;
    }
}
