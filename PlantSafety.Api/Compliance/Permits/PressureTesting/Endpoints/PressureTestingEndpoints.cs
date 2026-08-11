// ============================================================================
// File: PressureTestingEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for PressureTesting (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating PressureTesting to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when PressureTesting migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Compliance.Permits.PressureTesting.Authorization;

namespace PlantSafety.Api.Compliance.Permits.PressureTesting.Endpoints;

/// <summary>Minimal API endpoint group stub for PressureTesting.</summary>
public static class PressureTestingEndpoints
{
    /// <summary>
    /// Maps the PressureTesting Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapPressureTestingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/compliance/permits/pressuretesting")
            .RequireAuthorization(PressureTestingAuthorizationPolicies.Read)
            .WithTags("PressureTesting");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}