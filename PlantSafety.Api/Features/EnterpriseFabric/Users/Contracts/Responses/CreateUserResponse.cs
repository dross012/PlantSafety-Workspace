// ============================================================================
// File: CreateUserResponse.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Description: External-facing response DTO returned after successfully creating
//              a tenant user account.
// ============================================================================

namespace PlantSafety.Api.Features.EnterpriseFabric.Users.Contracts.Responses;

/// <summary>Response payload returned after creating a user.</summary>
/// <param name="UserId">Identifier of the newly created user.</param>
public sealed record CreateUserResponse(Guid UserId);
