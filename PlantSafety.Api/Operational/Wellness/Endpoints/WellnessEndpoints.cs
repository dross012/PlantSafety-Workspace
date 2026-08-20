// ============================================================================
// File: WellnessEndpoints.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-20
// Last Updated: 2026-08-20
// Description: Minimal API endpoint group stub for Wellness (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating Wellness to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when Wellness migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Operational.Wellness.Authorization;

namespace PlantSafety.Api.Operational.Wellness.Endpoints;

/// <summary>Minimal API endpoint group stub for Wellness.</summary>
public static class WellnessEndpoints
{
    /// <summary>
    /// Maps the Wellness Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapWellnessEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/operational/wellness")
            .RequireAuthorization(WellnessAuthorizationPolicies.Read)
            .WithTags("Wellness");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}