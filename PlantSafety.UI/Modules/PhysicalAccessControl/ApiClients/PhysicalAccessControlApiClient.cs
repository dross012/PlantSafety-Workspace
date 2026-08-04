// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Typed API client for the PhysicalAccessControl module. Wraps HTTP
//   communication with the real PlantSafety.Api endpoints under
//   api/v1/physicalaccesscontrol/... (PhysicalAccessControlV1Controller). Calls gracefully fault
//   through ApiResponseEnvelope so the UI degrades to its Error state on backend failure.
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
using PlantSafety.UI.Modules.PhysicalAccessControl.Models;

namespace PlantSafety.UI.Modules.PhysicalAccessControl.ApiClients;

/// <summary>Typed API client for the PhysicalAccessControl module.</summary>
public sealed class PhysicalAccessControlApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="PhysicalAccessControlApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public PhysicalAccessControlApiClient(HttpClient http, ILogger<PhysicalAccessControlApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves a single access point by identity.</summary>
    public Task<ApiResponseEnvelope<GetAccessPointByIdResponseDto>> GetAccessPointByIdAsync(
        Guid accessPointId, CancellationToken cancellationToken = default)
        => GetAsync<GetAccessPointByIdResponseDto>($"v1/physicalaccesscontrol/access-points/{accessPointId:D}", cancellationToken);

    /// <summary>Retrieves the current occupancy of a single safety zone, correlated from ingested access events,
    /// feeding the Live Access &amp; Occupancy Map dashboard.</summary>
    public Task<ApiResponseEnvelope<GetZoneOccupancyResponseDto>> GetZoneOccupancyAsync(
        Guid safetyZoneId, CancellationToken cancellationToken = default)
        => GetAsync<GetZoneOccupancyResponseDto>($"v1/physicalaccesscontrol/safety-zones/{safetyZoneId:D}/occupancy", cancellationToken);

    /// <summary>Lists access events for a given safety zone within a date range, feeding the Live Access &amp; Occupancy Map dashboard.</summary>
    public Task<ApiResponseEnvelope<ListAccessEventsByZoneResponseDto>> ListAccessEventsByZoneAsync(
        Guid safetyZoneId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var query = $"?SafetyZoneId={Uri.EscapeDataString(safetyZoneId.ToString("D", CultureInfo.InvariantCulture))}" +
                    $"&From={Uri.EscapeDataString(from.ToString("O", CultureInfo.InvariantCulture))}" +
                    $"&To={Uri.EscapeDataString(to.ToString("O", CultureInfo.InvariantCulture))}";
        return GetAsync<ListAccessEventsByZoneResponseDto>($"v1/physicalaccesscontrol/access-events{query}", cancellationToken);
    }

    /// <summary>Lists active (unresolved) anomalies for the Anomaly Feed dashboard.</summary>
    public Task<ApiResponseEnvelope<ListActiveAnomaliesResponseDto>> ListActiveAnomaliesAsync(
        Guid? siteId, CancellationToken cancellationToken = default)
    {
        var query = siteId.HasValue ? $"?SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}" : string.Empty;
        return GetAsync<ListActiveAnomaliesResponseDto>($"v1/physicalaccesscontrol/access-anomalies/active{query}", cancellationToken);
    }

    /// <summary>Retrieves the current lifecycle status of a single credential (badge), feeding the Credential Lifecycle Status dashboard.</summary>
    public Task<ApiResponseEnvelope<GetCredentialStatusResponseDto>> GetCredentialStatusAsync(
        Guid credentialId, CancellationToken cancellationToken = default)
        => GetAsync<GetCredentialStatusResponseDto>($"v1/physicalaccesscontrol/credentials/{credentialId:D}/status", cancellationToken);
}
