// ============================================================================
// File: CreateUserRequest.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing request DTO for POST /features/enterprise-fabric/users,
//              bound from the HTTP request body.
// ============================================================================

namespace PlantSafety.Api.Features.EnterpriseFabric.Users.Contracts.Requests;

/// <summary>Request body for creating a new tenant user account.</summary>
/// <param name="FirstName">First name. Max 100 characters.</param>
/// <param name="LastName">Last name. Max 100 characters.</param>
/// <param name="Email">Email address (unique within tenant). Used as login identity.</param>
/// <param name="Roles">Initial roles to assign. Must be valid role names.</param>
/// <param name="SiteIds">Site access list. Empty means no site-specific access.</param>
public sealed record CreateUserRequest(
    string             FirstName,
    string             LastName,
    string             Email,
    IReadOnlyList<string> Roles,
    IReadOnlyList<Guid>   SiteIds);
