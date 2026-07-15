// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Query to retrieve a paginated list of Safety Drills / Exercises.
//   Supports filtering by status, drill type, and site.
//   Returns PaginatedResult<DrillDto> per §XVIII canonical pagination envelope.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Features.SafetyEvents.Queries.DrillsAndExercises.List;

/// <summary>
/// Returns a paginated list of <see cref="Features.SafetyEvents.Models.DrillsAndExercises.DrillDto"/> records.
/// Results are scoped to the caller's <see cref="TenantId"/> — cross-tenant access is forbidden.
/// </summary>
public sealed record ListDrillsQuery : IQuery<PaginatedResult<Models.DrillsAndExercises.DrillDto>>
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

    // --- Pagination ---

    /// <summary>1-based page number. Clamped to [1, MaxPageSize].</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Items per page. Clamped to [1, 250].</summary>
    public int PageSize { get; init; } = 25;

    /// <summary>Field to order by. Defaults to <c>ScheduledAtUtc</c>.</summary>
    public string OrderBy { get; init; } = "ScheduledAtUtc";

    /// <summary>Sort direction: <c>ASC</c> or <c>DESC</c>.</summary>
    public string SortDirection { get; init; } = "DESC";

    // --- Filters ---

    /// <summary>Optional site filter.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional status filter (e.g., Scheduled, Completed, Cancelled).</summary>
    public string? Status { get; init; }

    /// <summary>Optional drill type filter (e.g., Evacuation, FireResponse, ChemicalSpill, MedicalEmergency).</summary>
    public string? DrillType { get; init; }
}
