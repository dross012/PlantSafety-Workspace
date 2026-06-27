// ============================================================================
// File: DrillsAndExercisesEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for DrillsAndExercises (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating DrillsAndExercises to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when DrillsAndExercises migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.SafetyEvents.DrillsAndExercises.Authorization;

namespace PlantSafety.Api.SafetyEvents.DrillsAndExercises.Endpoints;

/// <summary>Minimal API endpoint group stub for DrillsAndExercises.</summary>
public static class DrillsAndExercisesEndpoints
{
    /// <summary>
    /// Maps the DrillsAndExercises Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapDrillsAndExercisesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/safety-events/drills-and-exercises")
            .RequireAuthorization(DrillsAndExercisesAuthorizationPolicies.Read)
            .WithTags("DrillsAndExercises")
            .WithOpenApi();

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}