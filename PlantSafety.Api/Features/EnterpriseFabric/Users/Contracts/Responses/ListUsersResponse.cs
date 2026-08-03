// ============================================================================
// File: ListUsersResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-03
// Description: External-facing paged response DTO returned by GET
//              /features/enterprise-fabric/users.
// ============================================================================

using PlantSafety.Application.Features.EnterpriseFabric.Models.Users;

namespace PlantSafety.Api.Features.EnterpriseFabric.Users.Contracts.Responses;

/// <summary>Paged response payload for the user list.</summary>
/// <param name="Items">The page of users matching the query filters.</param>
/// <param name="TotalCount">Total number of users matching the filters, across all pages.</param>
/// <param name="PageNumber">The 1-based page number returned.</param>
/// <param name="PageSize">The page size used.</param>
public sealed record ListUsersResponse(
    IReadOnlyList<UserProfileDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);
