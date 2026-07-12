// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Query to retrieve a paginated list of Permits.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.Compliance.Models;

namespace PlantSafety.Application.Features.Compliance.Queries.Permits.List;

/// <summary>Returns a paginated list of <see cref="PermitDto"/> records for the caller's tenant.</summary>
public sealed record ListPermitsQuery : IQuery<PaginatedResult<PermitDto>>
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

    /// <summary>1-based page number.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Items per page.</summary>
    public int PageSize { get; init; } = 25;

    /// <summary>Sort field (ValidFromUtc, ValidToUtc, Status, PermitType).</summary>
    public string OrderBy { get; init; } = "ValidFromUtc";

    /// <summary>Sort direction.</summary>
    public string SortDirection { get; init; } = "DESC";

    /// <summary>Optional site filter.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional permit type filter.</summary>
    public string? PermitType { get; init; }

    /// <summary>Optional status filter.</summary>
    public string? Status { get; init; }

    /// <summary>Inclusive lower bound on validity start.</summary>
    public DateTimeOffset? ValidFromUtc { get; init; }

    /// <summary>Inclusive upper bound on validity end.</summary>
    public DateTimeOffset? ValidToUtc { get; init; }
}
