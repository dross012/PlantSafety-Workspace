// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Query to retrieve a paginated list of Near Miss records.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.SafetyEvents.Models.NearMisses;

namespace PlantSafety.Application.Features.SafetyEvents.Queries.NearMisses.List;

/// <summary>
/// Returns a paginated list of <see cref="NearMissDto"/> records for the caller's tenant.
/// </summary>
public sealed record ListNearMissesQuery : IQuery<PaginatedResult<NearMissDto>>
{
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

    /// <summary>1-based page number.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Items per page.</summary>
    public int PageSize { get; init; } = 25;

    /// <summary>Field to order by.</summary>
    public string OrderBy { get; init; } = "OccurredAtUtc";

    /// <summary>Sort direction: ASC or DESC.</summary>
    public string SortDirection { get; init; } = "DESC";

    /// <summary>Optional site filter.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional potential severity filter.</summary>
    public string? PotentialSeverity { get; init; }

    /// <summary>Optional hazard category filter.</summary>
    public string? HazardCategory { get; init; }

    /// <summary>Inclusive lower bound on occurrence date.</summary>
    public DateTimeOffset? OccurredFromUtc { get; init; }

    /// <summary>Inclusive upper bound on occurrence date.</summary>
    public DateTimeOffset? OccurredToUtc { get; init; }
}
