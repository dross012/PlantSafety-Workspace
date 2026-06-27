// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Query to retrieve a paginated list of Incidents.
//   Supports filtering by status, severity, date range, and site.
//   Returns PaginatedResult<IncidentSummaryDto> per §XVIII canonical pagination envelope.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Features.SafetyEvents.Queries.IncidentManagement.List;

/// <summary>
/// Returns a paginated list of <see cref="Features.SafetyEvents.Models.IncidentManagement.IncidentSummaryDto"/> records.
/// Results are scoped to the caller's <see cref="TenantId"/> — cross-tenant access is forbidden.
/// </summary>
public sealed record ListIncidentsQuery : IQuery<PaginatedResult<Models.IncidentManagement.IncidentSummaryDto>>
{
    // --- Canonical envelope ---

    /// <inheritdoc/>
    public required Guid TraceId { get; init; }

    /// <inheritdoc/>
    public required Guid SpanId { get; init; }

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

    // --- Pagination ---

    /// <summary>1-based page number. Clamped to [1, MaxPageSize].</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Items per page. Clamped to [1, 250].</summary>
    public int PageSize { get; init; } = 25;

    /// <summary>Field to order by. Defaults to <c>IncidentOccurredAtUtc</c>.</summary>
    public string OrderBy { get; init; } = "IncidentOccurredAtUtc";

    /// <summary>Sort direction: <c>ASC</c> or <c>DESC</c>.</summary>
    public string SortDirection { get; init; } = "DESC";

    // --- Filters ---

    /// <summary>Optional site filter.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional status filter (e.g., Reported, UnderInvestigation).</summary>
    public string? Status { get; init; }

    /// <summary>Optional severity filter.</summary>
    public string? Severity { get; init; }

    /// <summary>Inclusive lower bound on occurrence date.</summary>
    public DateTimeOffset? OccurredFromUtc { get; init; }

    /// <summary>Inclusive upper bound on occurrence date.</summary>
    public DateTimeOffset? OccurredToUtc { get; init; }
}
