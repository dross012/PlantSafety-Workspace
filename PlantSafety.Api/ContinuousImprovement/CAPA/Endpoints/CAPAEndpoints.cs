// ============================================================================
// File: CAPAEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for CAPA (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating CAPA to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when CAPA migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.ContinuousImprovement.CAPA.Authorization;

namespace PlantSafety.Api.ContinuousImprovement.CAPA.Endpoints;

/// <summary>Minimal API endpoint group stub for CAPA.</summary>
public static class CAPAEndpoints
{
    /// <summary>
    /// Maps the CAPA Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapCAPAEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/continuous-improvement/capa")
            .RequireAuthorization(CAPAAuthorizationPolicies.Read)
            .WithTags("CAPA")
            .WithOpenApi();

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}