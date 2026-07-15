// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Typed API client for the Incident Management module.
//   Wraps all HTTP communication for the Incidents List, Incident Detail
//   (Volume ID1), SLA Monitor (Volume ISM1), and Timeline (Volume IT1)
//   pages. All methods return ApiResponseEnvelope<T>. Targets the
//   canonical v1/safety-events/incidents REST surface, backed by
//   PlantSafety.Api.SafetyEvents.IncidentManagement.Controllers.V1
//   .IncidentsCanonicalV1Controller for List/GetById. The assign/escalate/
//   notes/resolve/timeline/sla-monitor endpoints below are not yet backed
//   by a controller — those calls surface as a Problem/Fault and the UI
//   degrades gracefully via ErrorBoundaryComponent in that case.
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
using PlantSafety.UI.Modules.IncidentManagement.Models;

namespace PlantSafety.UI.Modules.IncidentManagement.ApiClients;

/// <summary>Typed API client for the Incident Management module.</summary>
public sealed class IncidentManagementApiClient : SafetyApiClientBase
{
    /// <summary>Initializes a new instance of <see cref="IncidentManagementApiClient"/>.</summary>
    public IncidentManagementApiClient(HttpClient http, ILogger<IncidentManagementApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves a paginated page of incidents for the list view.</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<IncidentSummaryDto>>> GetIncidentsAsync(
        int pageNumber = 1, int pageSize = 25, string? severityFilter = null, string? statusFilter = null,
        CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<IncidentSummaryDto>>(
            $"v1/safety-events/incidents?pageNumber={pageNumber}&pageSize={pageSize}" +
            (string.IsNullOrWhiteSpace(severityFilter) ? string.Empty : $"&severity={Uri.EscapeDataString(severityFilter)}") +
            (string.IsNullOrWhiteSpace(statusFilter) ? string.Empty : $"&status={Uri.EscapeDataString(statusFilter)}"),
            cancellationToken);

    /// <summary>Retrieves the full detail for a single incident (Volume ID1).</summary>
    public Task<ApiResponseEnvelope<IncidentDetailDto>> GetIncidentAsync(Guid incidentId, CancellationToken cancellationToken = default)
        => GetAsync<IncidentDetailDto>($"v1/safety-events/incidents/{incidentId}", cancellationToken);

    /// <summary>Retrieves the chronological timeline for a single incident (Volume ID1/IT1).</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<IncidentTimelineEntryDto>>> GetTimelineAsync(
        Guid incidentId, CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<IncidentTimelineEntryDto>>($"v1/safety-events/incidents/{incidentId}/timeline", cancellationToken);

    /// <summary>Acknowledges/assigns a technician to an incident.</summary>
    public Task<ApiResponseEnvelope<IncidentDetailDto>> AssignTechnicianAsync(
        Guid incidentId, Guid technicianId, CancellationToken cancellationToken = default)
        => PostAsync<object, IncidentDetailDto>($"v1/safety-events/incidents/{incidentId}/assign", new { technicianId }, cancellationToken);

    /// <summary>Escalates an incident with a required reason.</summary>
    public Task<ApiResponseEnvelope<IncidentDetailDto>> EscalateAsync(
        Guid incidentId, string reason, CancellationToken cancellationToken = default)
        => PostAsync<object, IncidentDetailDto>($"v1/safety-events/incidents/{incidentId}/escalate", new { reason }, cancellationToken);

    /// <summary>Adds a technician/operator note to an incident.</summary>
    public Task<ApiResponseEnvelope<IncidentDetailDto>> AddNoteAsync(
        Guid incidentId, string text, CancellationToken cancellationToken = default)
        => PostAsync<object, IncidentDetailDto>($"v1/safety-events/incidents/{incidentId}/notes", new { text }, cancellationToken);

    /// <summary>Resolves an incident with a required final note.</summary>
    public Task<ApiResponseEnvelope<IncidentDetailDto>> ResolveAsync(
        Guid incidentId, string resolutionNotes, CancellationToken cancellationToken = default)
        => PostAsync<object, IncidentDetailDto>($"v1/safety-events/incidents/{incidentId}/resolve", new { resolutionNotes }, cancellationToken);

    /// <summary>Retrieves the SLA countdown grid rows for all active incidents (Volume ISM1).</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<SlaIncidentDto>>> GetSlaIncidentsAsync(CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<SlaIncidentDto>>("v1/safety-events/incidents/sla-monitor/incidents", cancellationToken);

    /// <summary>Retrieves the SLA risk panel rows grouped by threshold (Volume ISM1).</summary>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<SlaRiskDto>>> GetSlaRiskAsync(CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<SlaRiskDto>>("v1/safety-events/incidents/sla-monitor/risk", cancellationToken);

    /// <summary>Retrieves the SLA trend summary for the trend chart (Volume ISM1).</summary>
    public Task<ApiResponseEnvelope<SlaTrendDto>> GetSlaTrendAsync(CancellationToken cancellationToken = default)
        => GetAsync<SlaTrendDto>("v1/safety-events/incidents/sla-monitor/trend", cancellationToken);
}
