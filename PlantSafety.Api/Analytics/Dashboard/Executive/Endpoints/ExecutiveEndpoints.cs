// ============================================================================
// File: ExecutiveEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for Executive (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating Executive to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when Executive migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Analytics.Dashboard.Executive.Authorization;

namespace PlantSafety.Api.Analytics.Dashboard.Executive.Endpoints;

/// <summary>Minimal API endpoint group stub for Executive.</summary>
public static class ExecutiveEndpoints
{
    /// <summary>
    /// Maps the Executive Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapExecutiveEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/analytics/dashboard/executive")
            .RequireAuthorization(ExecutiveAuthorizationPolicies.Read)
            .WithTags("Executive");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}