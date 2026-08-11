// ============================================================================
// File: PermitsToWorkEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for PermitsToWork (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating PermitsToWork to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when PermitsToWork migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Operational.PermitsToWork.Authorization;

namespace PlantSafety.Api.Operational.PermitsToWork.Endpoints;

/// <summary>Minimal API endpoint group stub for PermitsToWork.</summary>
public static class PermitsToWorkEndpoints
{
    /// <summary>
    /// Maps the PermitsToWork Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapPermitsToWorkEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/operational/permits-to-work")
            .RequireAuthorization(PermitsToWorkAuthorizationPolicies.Read)
            .WithTags("PermitsToWork");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}