// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Query returning the SLA countdown grid rows for all active (non-Resolved,
//   non-Archived) incidents. Backs v1/safety-events/incidents/sla-monitor/incidents (UI Gaps
//   Scoping, IncidentManagement Part 2).
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.SlaMonitoring.Models;

namespace PlantSafety.Application.SafetyEvents.SlaMonitoring.Queries;

/// <summary>Returns the SLA countdown grid rows for all active incidents in the caller's tenant.</summary>
public sealed record GetSlaMonitorIncidentsQuery : IQuery<PaginatedResult<SlaIncidentDto>>
{
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

    /// <summary>1-based page number. Clamped to [1, MaxPageSize].</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Items per page. Clamped to [1, 250].</summary>
    public int PageSize { get; init; } = 25;
}
