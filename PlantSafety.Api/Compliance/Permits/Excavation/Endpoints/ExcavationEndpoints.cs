// ============================================================================
// File: ExcavationEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for Excavation (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating Excavation to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when Excavation migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Compliance.Permits.Excavation.Authorization;

namespace PlantSafety.Api.Compliance.Permits.Excavation.Endpoints;

/// <summary>Minimal API endpoint group stub for Excavation.</summary>
public static class ExcavationEndpoints
{
    /// <summary>
    /// Maps the Excavation Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapExcavationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/compliance/permits/excavation")
            .RequireAuthorization(ExcavationAuthorizationPolicies.Read)
            .WithTags("Excavation");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}