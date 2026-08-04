// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Typed API client for the AutonomousVehicleSafety module. Wraps HTTP
//   communication with the real PlantSafety.Api endpoints under
//   api/v1/autonomousvehiclesafety/... (AutonomousVehicleSafetyV1Controller). Calls gracefully
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
using PlantSafety.UI.Modules.AutonomousVehicleSafety.Models;

namespace PlantSafety.UI.Modules.AutonomousVehicleSafety.ApiClients;

/// <summary>Typed API client for the AutonomousVehicleSafety module.</summary>
public sealed class AutonomousVehicleSafetyApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="AutonomousVehicleSafetyApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public AutonomousVehicleSafetyApiClient(HttpClient http, ILogger<AutonomousVehicleSafetyApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves a single autonomous vehicle asset by identifier, feeding the Autonomous Fleet Status lookup panel.</summary>
    public Task<ApiResponseEnvelope<AutonomousVehicleAssetDto>> GetAutonomousVehicleAssetByIdAsync(
        Guid autonomousVehicleAssetId, CancellationToken cancellationToken = default)
        => GetAsync<AutonomousVehicleAssetDto>($"v1/autonomousvehiclesafety/assets/{autonomousVehicleAssetId:D}", cancellationToken);

    /// <summary>Returns geofences with active violations for a tenant/site within a date range,
    /// feeding the Autonomous Fleet Status &amp; Geofence Map dashboard.</summary>
    public Task<ApiResponseEnvelope<OperatingGeofenceResponseDto>> ListActiveGeofenceViolationsAsync(
        Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => GetAsync<OperatingGeofenceResponseDto>($"v1/autonomousvehiclesafety/geofences/active-violations{BuildDateRangeQuery(siteId, from, to)}", cancellationToken);

    /// <summary>Returns the current safety-case validation status for an autonomous vehicle asset,
    /// feeding the Safety Case Currency Board dashboard.</summary>
    public Task<ApiResponseEnvelope<SafetyCaseValidationDto>> GetSafetyCaseStatusAsync(
        Guid autonomousVehicleAssetId, CancellationToken cancellationToken = default)
        => GetAsync<SafetyCaseValidationDto>($"v1/autonomousvehiclesafety/safety-cases/status/{autonomousVehicleAssetId:D}", cancellationToken);

    /// <summary>Returns HumanRobotInteractionEvent records for a tenant/site within a date range,
    /// feeding the Human-Robot Interaction Heatmap dashboard.</summary>
    public Task<ApiResponseEnvelope<HumanRobotInteractionEventResponseDto>> ListHumanRobotInteractionEventsAsync(
        Guid? siteId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => GetAsync<HumanRobotInteractionEventResponseDto>($"v1/autonomousvehiclesafety/human-robot-interactions{BuildDateRangeQuery(siteId, from, to)}", cancellationToken);

    private static string BuildDateRangeQuery(Guid? siteId, DateTime from, DateTime to)
    {
        var query = $"?From={Uri.EscapeDataString(from.ToString("O", CultureInfo.InvariantCulture))}" +
                    $"&To={Uri.EscapeDataString(to.ToString("O", CultureInfo.InvariantCulture))}";
        if (siteId.HasValue)
            query += $"&SiteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}";
        return query;
    }
}
