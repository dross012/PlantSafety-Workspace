// ============================================================================
// File: ListUsersRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-03
// Description: External-facing request DTO for GET /features/enterprise-fabric/users,
//              bound from the HTTP query string.
// ============================================================================

namespace PlantSafety.Api.Features.EnterpriseFabric.Users.Contracts.Requests;

/// <summary>Query parameters for listing users within the tenant.</summary>
/// <param name="Role">Optional role filter.</param>
/// <param name="SiteId">Optional site filter.</param>
/// <param name="IsActive">Optional active/inactive filter. Omit to return both.</param>
/// <param name="OrderBy">Column to order by. Defaults to "UserId".</param>
/// <param name="SortDirection">Sort direction ("ASC" or "DESC"). Defaults to "ASC".</param>
/// <param name="PageNumber">1-based page number. Defaults to 1.</param>
/// <param name="PageSize">Page size. Defaults to 25.</param>
public sealed record ListUsersRequest(
    string? Role = null,
    Guid? SiteId = null,
    bool? IsActive = null,
    string OrderBy = "UserId",
    string SortDirection = "ASC",
    int PageNumber = 1,
    int PageSize = 25);
