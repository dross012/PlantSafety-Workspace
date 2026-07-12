// ============================================================================
// File: AnalyticsEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Minimal API endpoint group stub for Integrations.Analytics (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating Integrations.Analytics to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when Integrations.Analytics migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Integrations.Analytics.Authorization;

namespace PlantSafety.Api.Integrations.Analytics.Endpoints;

/// <summary>Minimal API endpoint group stub for Integrations.Analytics.</summary>
public static class AnalyticsEndpoints
{
    /// <summary>
    /// Maps the Integrations.Analytics Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/integrations/analytics")
            .RequireAuthorization(AnalyticsAuthorizationPolicies.Read)
            .WithTags("Analytics")
            .WithOpenApi();

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}
