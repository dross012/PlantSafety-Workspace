// ============================================================================
// File: ReportingEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for Reporting (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating Reporting to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when Reporting migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Analytics.Reporting.Authorization;

namespace PlantSafety.Api.Analytics.Reporting.Endpoints;

/// <summary>Minimal API endpoint group stub for Reporting.</summary>
public static class ReportingEndpoints
{
    /// <summary>
    /// Maps the Reporting Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapReportingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/analytics/reporting")
            .RequireAuthorization(ReportingAuthorizationPolicies.Read)
            .WithTags("Reporting")
            .WithOpenApi();

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}