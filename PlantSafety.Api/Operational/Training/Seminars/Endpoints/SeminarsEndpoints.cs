// ============================================================================
// File: SeminarsEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for Seminars (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating Seminars to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when Seminars migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Operational.Training.Seminars.Authorization;

namespace PlantSafety.Api.Operational.Training.Seminars.Endpoints;

/// <summary>Minimal API endpoint group stub for Seminars.</summary>
public static class SeminarsEndpoints
{
    /// <summary>
    /// Maps the Seminars Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapSeminarsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/operational/training/seminars")
            .RequireAuthorization(SeminarsAuthorizationPolicies.Read)
            .WithTags("Seminars")
            .WithOpenApi();

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}