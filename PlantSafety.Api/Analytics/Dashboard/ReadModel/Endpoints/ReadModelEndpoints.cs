// ============================================================================
// File: ReadModelEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for ReadModel (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating ReadModel to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when ReadModel migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Analytics.Dashboard.ReadModel.Authorization;

namespace PlantSafety.Api.Analytics.Dashboard.ReadModel.Endpoints;

/// <summary>Minimal API endpoint group stub for ReadModel.</summary>
public static class ReadModelEndpoints
{
    /// <summary>
    /// Maps the ReadModel Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapReadModelEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/analytics/dashboard/readmodel")
            .RequireAuthorization(ReadModelAuthorizationPolicies.Read)
            .WithTags("ReadModel");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}