// ============================================================================
// File: SafetyEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for Safety (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating Safety to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when Safety migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Compliance.Procedures.Safety.Authorization;

namespace PlantSafety.Api.Compliance.Procedures.Safety.Endpoints;

/// <summary>Minimal API endpoint group stub for Safety.</summary>
public static class SafetyEndpoints
{
    /// <summary>
    /// Maps the Safety Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapSafetyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/compliance/procedures/safety")
            .RequireAuthorization(SafetyAuthorizationPolicies.Read)
            .WithTags("Safety");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}