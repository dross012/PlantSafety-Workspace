// ============================================================================
// File: HotTapEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for HotTap (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating HotTap to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when HotTap migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Compliance.Permits.HotTap.Authorization;

namespace PlantSafety.Api.Compliance.Permits.HotTap.Endpoints;

/// <summary>Minimal API endpoint group stub for HotTap.</summary>
public static class HotTapEndpoints
{
    /// <summary>
    /// Maps the HotTap Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapHotTapEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/compliance/permits/hottap")
            .RequireAuthorization(HotTapAuthorizationPolicies.Read)
            .WithTags("HotTap");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}