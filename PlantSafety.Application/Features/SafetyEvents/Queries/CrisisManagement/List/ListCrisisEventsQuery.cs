// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Query to retrieve a paginated list of Crisis Management events.
//   Supports filtering by status, crisis category, and site.
//   Returns PaginatedResult<CrisisEventDto> per §XVIII canonical pagination envelope.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Features.SafetyEvents.Queries.CrisisManagement.List;

/// <summary>
/// Returns a paginated list of <see cref="Features.SafetyEvents.Models.CrisisManagement.CrisisEventDto"/> records.
/// Results are scoped to the caller's <see cref="TenantId"/> — cross-tenant access is forbidden.
/// </summary>
public sealed record ListCrisisEventsQuery : IQuery<PaginatedResult<Models.CrisisManagement.CrisisEventDto>>
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

    /// <summary>Field to order by. Defaults to <c>DeclaredAtUtc</c>.</summary>
    public string OrderBy { get; init; } = "DeclaredAtUtc";

    /// <summary>Sort direction: <c>ASC</c> or <c>DESC</c>.</summary>
    public string SortDirection { get; init; } = "DESC";

    // --- Filters ---

    /// <summary>Optional site filter.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional status filter (e.g., Active, Contained, Resolved, ClosedOut).</summary>
    public string? Status { get; init; }

    /// <summary>Optional crisis category filter (e.g., Fire, ChemicalSpill, StructuralFailure, MassInjury).</summary>
    public string? CrisisCategory { get; init; }
}
