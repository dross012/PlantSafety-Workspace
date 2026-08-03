// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Query to fetch the paginated chronological timeline for a single Incident.
//   Backs the v1/safety-events/incidents/{id}/timeline route on IncidentsCanonicalV1Controller.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.SafetyEvents.Models.IncidentManagement;

namespace PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.Timeline;

/// <summary>Returns a paginated, chronologically-ordered timeline for a single incident.</summary>
public sealed record GetIncidentTimelineQuery : IQuery<PaginatedResult<IncidentTimelineEntryDto>>
{
    // --- Canonical envelope ---

    /// <inheritdoc/>
    public required string TraceId { get; init; }

    /// <inheritdoc/>
    public required string SpanId { get; init; }

    /// <inheritdoc/>
    public required string ApiVersionToken { get; init; }

    /// <inheritdoc/>
    public required Guid RequestingUserId { get; init; }

    /// <inheritdoc/>
    public required string RequestingUserRole { get; init; }

    /// <inheritdoc/>
    public required Guid TenantId { get; init; }

    /// <inheritdoc/>
    public required Guid CorrelationId { get; init; }

    /// <inheritdoc/>
    public CachePreference CachePreference { get; init; } = CachePreference.None;

    /// <inheritdoc/>
    public bool SuppressUnauthorized { get; init; }

    /// <inheritdoc/>
    public bool StrictMode { get; init; }

    // --- Domain payload ---

    /// <summary>Incident whose timeline is being retrieved.</summary>
    public required Guid IncidentId { get; init; }

    /// <summary>1-based page number. Clamped to [1, MaxPageSize].</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Items per page. Clamped to [1, 250].</summary>
    public int PageSize { get; init; } = 25;
}
