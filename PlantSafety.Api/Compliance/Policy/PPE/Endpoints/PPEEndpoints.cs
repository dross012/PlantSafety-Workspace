// ============================================================================
// File: PPEEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for PPE (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating PPE to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when PPE migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Compliance.Policy.PPE.Authorization;

namespace PlantSafety.Api.Compliance.Policy.PPE.Endpoints;

/// <summary>Minimal API endpoint group stub for PPE.</summary>
public static class PPEEndpoints
{
    /// <summary>
    /// Maps the PPE Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapPPEEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/compliance/policy/ppe")
            .RequireAuthorization(PPEAuthorizationPolicies.Read)
            .WithTags("PPE")
            .WithOpenApi();

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}