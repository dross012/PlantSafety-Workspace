// ============================================================================
// File: PredictiveSafetyEndpoints.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Minimal API endpoint group stub for PredictiveSafety (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating PredictiveSafety to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when PredictiveSafety migrates to Minimal API.
//
// ============================================================================
using PlantSafety.Api.Intelligence.PredictiveSafety.Authorization;

namespace PlantSafety.Api.Intelligence.PredictiveSafety.Endpoints;

/// <summary>Minimal API endpoint group stub for PredictiveSafety.</summary>
public static class PredictiveSafetyEndpoints
{
    /// <summary>
    /// Maps the PredictiveSafety Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapPredictiveSafetyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/intelligence/predictivesafety")
            .RequireAuthorization(PredictiveSafetyAuthorizationPolicies.Read)
            .WithTags("PredictiveSafety")
            .WithOpenApi();

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}