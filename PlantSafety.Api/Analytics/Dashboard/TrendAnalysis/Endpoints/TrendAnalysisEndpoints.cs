// ============================================================================
// File: TrendAnalysisEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for TrendAnalysis (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating TrendAnalysis to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when TrendAnalysis migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Analytics.Dashboard.TrendAnalysis.Authorization;

namespace PlantSafety.Api.Analytics.Dashboard.TrendAnalysis.Endpoints;

/// <summary>Minimal API endpoint group stub for TrendAnalysis.</summary>
public static class TrendAnalysisEndpoints
{
    /// <summary>
    /// Maps the TrendAnalysis Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapTrendAnalysisEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/analytics/dashboard/trendanalysis")
            .RequireAuthorization(TrendAnalysisAuthorizationPolicies.Read)
            .WithTags("TrendAnalysis")
            .WithOpenApi();

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}