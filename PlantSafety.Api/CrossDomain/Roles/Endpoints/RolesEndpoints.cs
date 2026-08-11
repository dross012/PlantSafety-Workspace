// ============================================================================
// File: RolesEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for Roles (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating Roles to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when Roles migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.CrossDomain.Roles.Authorization;

namespace PlantSafety.Api.CrossDomain.Roles.Endpoints;

/// <summary>Minimal API endpoint group stub for Roles.</summary>
public static class RolesEndpoints
{
    /// <summary>
    /// Maps the Roles Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapRolesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/cross-domain/roles")
            .RequireAuthorization(RolesAuthorizationPolicies.Read)
            .WithTags("Roles");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}