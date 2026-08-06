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
using System.Net.Http.Json;
using System.Text.Json;
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
    private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Initializes a new instance of <see cref="IncidentManagementApiClient"/>.</summary>
    public IncidentManagementApiClient(HttpClient http, ILogger<IncidentManagementApiClient> logger)
        : base(http, logger)
    {
    }

    /// <summary>Retrieves a paginated, filterable page of incidents for the list view.</summary>
    /// <remarks>
    /// Backed by GET <c>v1/safety-events/incidents</c> (PlantSafety.Api.SafetyEvents.
    /// IncidentManagement.Controllers.V1.IncidentsCanonicalV1Controller.ListAsync), which supports
    /// every filter exposed on <c>ListIncidentsCanonicalRequest</c>: severity, status, site, and an
    /// inclusive occurred-date range.
    /// </remarks>
    public Task<ApiResponseEnvelope<PaginatedResponseDto<IncidentSummaryDto>>> GetIncidentsAsync(
        int pageNumber = 1, int pageSize = 25, string? severityFilter = null, string? statusFilter = null,
        Guid? siteId = null, DateTimeOffset? occurredFromUtc = null, DateTimeOffset? occurredToUtc = null,
        CancellationToken cancellationToken = default)
        => GetAsync<PaginatedResponseDto<IncidentSummaryDto>>(
            $"v1/safety-events/incidents?pageNumber={pageNumber}&pageSize={pageSize}" +
            (string.IsNullOrWhiteSpace(severityFilter) ? string.Empty : $"&severity={Uri.EscapeDataString(severityFilter)}") +
            (string.IsNullOrWhiteSpace(statusFilter) ? string.Empty : $"&status={Uri.EscapeDataString(statusFilter)}") +
            (siteId.HasValue ? $"&siteId={siteId.Value:D}" : string.Empty) +
            (occurredFromUtc.HasValue ? $"&occurredFromUtc={Uri.EscapeDataString(occurredFromUtc.Value.ToString("O"))}" : string.Empty) +
            (occurredToUtc.HasValue ? $"&occurredToUtc={Uri.EscapeDataString(occurredToUtc.Value.ToString("O"))}" : string.Empty),
            cancellationToken);

    /// <summary>
    /// Creates a new Incident. Backed by POST <c>v1/features/safety-events/incidents</c>
    /// (PlantSafety.Api.Features.SafetyEvents.IncidentManagement.Controllers.V1.
    /// IncidentsV1Controller.CreateAsync, dispatches the real CreateIncidentCommand).
    /// </summary>
    public Task<ApiResponseEnvelope<Guid>> CreateIncidentAsync(
        CreateIncidentRequest request, CancellationToken cancellationToken = default)
        => PostAsync<CreateIncidentRequest, Guid>("v1/features/safety-events/incidents", request, cancellationToken);

    /// <summary>
    /// Updates the mutable fields of an existing Incident. Backed by PUT
    /// <c>v1/features/safety-events/incidents/{incidentId}</c> (dispatches the real
    /// UpdateIncidentCommand). An ETag is required for optimistic concurrency.
    /// </summary>
    public Task<ApiResponseEnvelope<Guid>> UpdateIncidentAsync(
        Guid incidentId, UpdateIncidentRequest request, CancellationToken cancellationToken = default)
        => PutAsync<UpdateIncidentRequest, Guid>($"v1/features/safety-events/incidents/{incidentId}", request, cancellationToken);

    /// <summary>
    /// Soft-deletes (archives) an Incident. Backed by DELETE
    /// <c>v1/features/safety-events/incidents/{incidentId}</c> (dispatches the real
    /// DeleteIncidentCommand). Uses a raw <see cref="HttpRequestMessage"/> because DELETE with a
    /// JSON body (DeletionReason/ETag) is not supported by the shared <see cref="SafetyApiClientBase"/>
    /// helper — mirrors the precedent established in <c>TenantAdminApiClient.DeleteWithBodyAsync</c>.
    /// </summary>
    public async Task<ApiResponseEnvelope<Guid>> DeleteIncidentAsync(
        Guid incidentId, DeleteIncidentRequest request, CancellationToken cancellationToken = default)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"v1/features/safety-events/incidents/{incidentId}")
        {
            Content = JsonContent.Create(request, options: _serializerOptions),
        };

        try
        {
            var response = await Http.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var wire = await response.Content
                    .ReadFromJsonAsync<ApiEnvelopeWireDto<Guid>>(_serializerOptions, cancellationToken)
                    .ConfigureAwait(false);
                return ApiResponseEnvelope<Guid>.Success(wire!.Data);
            }

            ProblemDetailsDto? problem = null;
            try
            {
                problem = await response.Content
                    .ReadFromJsonAsync<ProblemDetailsDto>(_serializerOptions, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch
            {
                // Non-JSON error body — fall through to the unknown-problem fallback below.
            }

            return ApiResponseEnvelope<Guid>.FromProblem(problem ?? ProblemDetailsDto.Unknown((int)response.StatusCode));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ApiResponseEnvelope<Guid>.Fault(ex.GetType().Name, ex.Message);
        }
    }

    /// <summary>
    /// Mirrors the server-side <c>PlantSafety.Api.Models.ApiResponse&lt;T&gt;</c> envelope shape,
    /// used only to deserialize the "data" property of the delete response.
    /// </summary>
    private sealed class ApiEnvelopeWireDto<T>
    {
        public bool Success { get; init; }
        public T Data { get; init; } = default!;
    }

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
