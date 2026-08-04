// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-04
// FILE DESCRIPTION: Typed API client for the Local Emergency Coordination module. Wraps HTTP
//   communication for the read endpoints under /v1/safety-events/local-emergency-coordination,
//   backed by PlantSafety.Api.SafetyEvents.LocalEmergencyCoordination.Controllers.V1
//   .LocalEmergencyCoordinationV1Controller.
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
using PlantSafety.UI.Modules.LocalEmergencyCoordination.Models;

namespace PlantSafety.UI.Modules.LocalEmergencyCoordination.ApiClients;

/// <summary>Typed API client for the Local Emergency Coordination module.</summary>
public sealed class LocalEmergencyCoordinationApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="LocalEmergencyCoordinationApiClient"/>.</summary>
    /// <param name="http">Typed HttpClient with correlation and auth handler pipeline.</param>
    /// <param name="logger">SIEM-safe structured logger.</param>
    public LocalEmergencyCoordinationApiClient(HttpClient http, ILogger<LocalEmergencyCoordinationApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves mutual aid agreements for the Mutual Aid Agreement Registry dashboard.</summary>
    /// <param name="siteId">Optional site filter.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<MutualAidAgreementListResponse>> ListMutualAidAgreementsAsync(
        Guid? siteId = null,
        CancellationToken cancellationToken = default)
        => GetAsync<MutualAidAgreementListResponse>(
            siteId.HasValue
                ? $"v1/safety-events/local-emergency-coordination/mutual-aid-agreements?siteId={siteId}"
                : "v1/safety-events/local-emergency-coordination/mutual-aid-agreements",
            cancellationToken);

    /// <summary>Retrieves joint response plans coordinated for the given incident (Active Joint Response Status dashboard).</summary>
    /// <param name="incidentId">The SafetyEvents incident identifier.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<JointResponsePlanListResponse>> GetJointResponsePlanByIncidentAsync(
        Guid incidentId,
        CancellationToken cancellationToken = default)
        => GetAsync<JointResponsePlanListResponse>(
            $"v1/safety-events/local-emergency-coordination/incidents/{incidentId}/joint-response-plans",
            cancellationToken);

    /// <summary>Retrieves joint post-incident debrief records recorded within a date range (Post-Incident Debrief Summary dashboard).</summary>
    /// <param name="from">Inclusive start of the date range (UTC).</param>
    /// <param name="to">Inclusive end of the date range (UTC).</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    public Task<ApiResponseEnvelope<JointDebriefListResponse>> ListJointDebriefsByPeriodAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
        => GetAsync<JointDebriefListResponse>(
            $"v1/safety-events/local-emergency-coordination/debriefs?from={from:O}&to={to:O}",
            cancellationToken);
}
