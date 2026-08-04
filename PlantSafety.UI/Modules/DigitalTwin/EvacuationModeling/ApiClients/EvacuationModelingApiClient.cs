// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Typed API client for the DigitalTwin.EvacuationModeling module. Targets
//   PlantSafety.Api's /api/v1/digitaltwin/evacuation-modeling endpoints.
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
using PlantSafety.UI.Modules.DigitalTwin.EvacuationModeling.Models;

namespace PlantSafety.UI.Modules.DigitalTwin.EvacuationModeling.ApiClients;

/// <summary>Typed API client for the DigitalTwin.EvacuationModeling module.</summary>
public sealed class EvacuationModelingApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="EvacuationModelingApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public EvacuationModelingApiClient(HttpClient http, ILogger<EvacuationModelingApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves all currently open (uncleared) route obstructions for the active site, if any.</summary>
    /// <param name="siteId">Optional site filter.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<RouteObstructionDto[]>> GetActiveObstructionsAsync(
        Guid? siteId, CancellationToken cancellationToken = default)
    {
        var query = siteId.HasValue ? $"?siteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}" : string.Empty;
        return GetAsync<RouteObstructionDto[]>($"v1/digitaltwin/evacuation-modeling/routes/active-obstructions{query}", cancellationToken);
    }

    /// <summary>Retrieves evacuation bottleneck history, optionally scoped to a route.</summary>
    /// <param name="siteId">Optional site filter.</param>
    /// <param name="routeId">Optional route filter.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<EvacuationBottleneckDto[]>> GetBottleneckHistoryAsync(
        Guid? siteId, Guid? routeId, CancellationToken cancellationToken = default)
    {
        var query = $"?siteId={(siteId.HasValue ? Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture)) : string.Empty)}" +
                    $"&routeId={(routeId.HasValue ? Uri.EscapeDataString(routeId.Value.ToString("D", CultureInfo.InvariantCulture)) : string.Empty)}";
        return GetAsync<EvacuationBottleneckDto[]>($"v1/digitaltwin/evacuation-modeling/bottlenecks{query}", cancellationToken);
    }

    /// <summary>Retrieves the most recently started egress-simulation run for a zone.</summary>
    /// <param name="zoneId">The evacuation zone identifier.</param>
    /// <param name="siteId">Optional site filter.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<EgressSimulationDto>> GetLatestEgressSimulationAsync(
        Guid zoneId, Guid? siteId, CancellationToken cancellationToken = default)
    {
        var query = siteId.HasValue ? $"?siteId={Uri.EscapeDataString(siteId.Value.ToString("D", CultureInfo.InvariantCulture))}" : string.Empty;
        return GetAsync<EgressSimulationDto>($"v1/digitaltwin/evacuation-modeling/zones/{zoneId:D}/simulations/latest{query}", cancellationToken);
    }

    /// <summary>Retrieves the modeled evacuation routes (with nominal/planned egress times) for a zone,
    /// feeding the Egress Simulation Comparison dashboard's "planned" side.</summary>
    /// <param name="zoneId">The evacuation zone identifier.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<EvacuationRouteDto[]>> ListEvacuationRoutesByZoneAsync(
        Guid zoneId, CancellationToken cancellationToken = default)
        => GetAsync<EvacuationRouteDto[]>($"v1/digitaltwin/evacuation-modeling/zones/{zoneId:D}/routes", cancellationToken);
}
