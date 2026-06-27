// ============================================================================
// File: EngagementEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: Minimal API endpoint group stub for Engagement Analytics (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface. This class provides a forward-compatible hook for Minimal API migration.
//
// Rationale:
// - Provides a registration point for migrating Engagement to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when Engagement migrates to Minimal API. Use TypedResults for compile-time response type safety.
//
// ============================================================================

using PlantSafety.Api.Analytics.Engagement.Authorization;

namespace PlantSafety.Api.Analytics.Engagement.Endpoints;

/// <summary>Minimal API endpoint group stub for Engagement Analytics.</summary>
public static class EngagementEndpoints
{
    /// <summary>
    /// Maps the Engagement Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapEngagementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/analytics/engagement")
            .RequireAuthorization(EngagementAuthorizationPolicies.Read)
            .WithTags("Analytics - Engagement")
            .WithOpenApi();

        // Stub â€” no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}