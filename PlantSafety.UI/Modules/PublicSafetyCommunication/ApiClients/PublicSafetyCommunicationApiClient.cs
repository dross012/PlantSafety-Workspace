// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Typed API client for the PublicSafetyCommunication module. Wraps HTTP
//   communication with the real PlantSafety.Api endpoints under
//   api/v1/publicsafetycommunication/... (PublicSafetyCommunicationV1Controller). Calls
//   gracefully fault through ApiResponseEnvelope so the UI degrades to its Error state on backend
//   failure.
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
using PlantSafety.UI.Modules.PublicSafetyCommunication.Models;

namespace PlantSafety.UI.Modules.PublicSafetyCommunication.ApiClients;

/// <summary>Typed API client for the PublicSafetyCommunication module.</summary>
public sealed class PublicSafetyCommunicationApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="PublicSafetyCommunicationApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public PublicSafetyCommunicationApiClient(HttpClient http, ILogger<PublicSafetyCommunicationApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Returns the public-safety agency directory, feeding the Agency Notification Status Board dashboard.</summary>
    public Task<ApiResponseEnvelope<ListPublicSafetyAgenciesResponseDto>> ListPublicSafetyAgenciesAsync(
        Guid? siteId, bool? activeOnly, CancellationToken cancellationToken = default)
    {
        var query = siteId.HasValue ? $"?siteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}" : string.Empty;
        if (activeOnly.HasValue)
            query += (query.Length == 0 ? "?" : "&") + $"activeOnly={activeOnly.Value.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()}";

        return GetAsync<ListPublicSafetyAgenciesResponseDto>($"v1/publicsafetycommunication/agencies{query}", cancellationToken);
    }

    /// <summary>Returns IncidentNotification dispatch status, feeding the Agency Notification Status Board's
    /// dispatch-latency and acknowledgement-rate KPIs.</summary>
    public Task<ApiResponseEnvelope<GetIncidentNotificationStatusResponseDto>> GetIncidentNotificationStatusAsync(
        Guid? siteId, Guid? incidentId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var query = $"?From={Uri.EscapeDataString(from.ToString("O", CultureInfo.InvariantCulture))}" +
                    $"&To={Uri.EscapeDataString(to.ToString("O", CultureInfo.InvariantCulture))}";
        if (siteId.HasValue)
            query += $"&SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";
        if (incidentId.HasValue)
            query += $"&IncidentId={Uri.EscapeDataString(incidentId.Value.ToString("D", CultureInfo.InvariantCulture))}";

        return GetAsync<GetIncidentNotificationStatusResponseDto>($"v1/publicsafetycommunication/notifications{query}", cancellationToken);
    }

    /// <summary>Returns active broadcasts, feeding the Broadcast History dashboard.</summary>
    public Task<ApiResponseEnvelope<ListActiveBroadcastsResponseDto>> ListActiveBroadcastsAsync(
        Guid? siteId, CancellationToken cancellationToken = default)
    {
        var query = siteId.HasValue ? $"?siteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}" : string.Empty;
        return GetAsync<ListActiveBroadcastsResponseDto>($"v1/publicsafetycommunication/broadcasts/active{query}", cancellationToken);
    }

    /// <summary>Returns the full communication log for an incident, feeding the After-Action Communication Timeline dashboard.</summary>
    public Task<ApiResponseEnvelope<GetCommunicationLogByIncidentResponseDto>> GetCommunicationLogByIncidentAsync(
        Guid incidentId, CancellationToken cancellationToken = default)
        => GetAsync<GetCommunicationLogByIncidentResponseDto>($"v1/publicsafetycommunication/communication-log/{incidentId:D}", cancellationToken);
}
