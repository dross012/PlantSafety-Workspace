// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Query to list users for a tenant, paged, with optional role/site/active
//   filters. Backs the Admin Portal's User Management page (Phase 7 §6.1).
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.EnterpriseFabric.Models.Users;

namespace PlantSafety.Application.Features.EnterpriseFabric.Queries.Users.List;

/// <summary>Returns a paged, filterable list of users for a tenant.</summary>
public sealed record ListUsersQuery : IQuery<PagedUsersResult>
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

    /// <summary>Optional role filter.</summary>
    public string? Role { get; init; }

    /// <summary>Optional site filter.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional active/inactive filter. Null returns both.</summary>
    public bool? IsActive { get; init; }

    /// <summary>Column to order by. Defaults to "UserId".</summary>
    public string OrderBy { get; init; } = "UserId";

    /// <summary>Sort direction ("ASC" or "DESC"). Defaults to "ASC".</summary>
    public string SortDirection { get; init; } = "ASC";

    /// <summary>1-based page number. Defaults to 1.</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Page size. Defaults to 25.</summary>
    public int PageSize { get; init; } = 25;
}

/// <summary>Paged result set of <see cref="UserProfileDto"/> records.</summary>
public sealed record PagedUsersResult
{
    /// <summary>The page of users matching the query filters.</summary>
    public required IReadOnlyList<UserProfileDto> Items { get; init; }

    /// <summary>Total number of users matching the filters, across all pages.</summary>
    public required int TotalCount { get; init; }

    /// <summary>The 1-based page number returned.</summary>
    public required int PageNumber { get; init; }

    /// <summary>The page size used.</summary>
    public required int PageSize { get; init; }
}
