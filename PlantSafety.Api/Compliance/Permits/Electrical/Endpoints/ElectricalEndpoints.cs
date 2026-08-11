// ============================================================================
// File: ElectricalEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for Electrical (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating Electrical to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when Electrical migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Compliance.Permits.Electrical.Authorization;

namespace PlantSafety.Api.Compliance.Permits.Electrical.Endpoints;

/// <summary>Minimal API endpoint group stub for Electrical.</summary>
public static class ElectricalEndpoints
{
    /// <summary>
    /// Maps the Electrical Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapElectricalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/compliance/permits/electrical")
            .RequireAuthorization(ElectricalAuthorizationPolicies.Read)
            .WithTags("Electrical");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}