// ============================================================================
// File: WorkforceSafetyEndpoints.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-20
// Last Updated: 2026-08-20
// Description: Minimal API endpoint group stub for WorkforceSafety (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating WorkforceSafety to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when WorkforceSafety migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Operational.WorkforceSafety.Authorization;

namespace PlantSafety.Api.Operational.WorkforceSafety.Endpoints;

/// <summary>Minimal API endpoint group stub for WorkforceSafety.</summary>
public static class WorkforceSafetyEndpoints
{
    /// <summary>
    /// Maps the WorkforceSafety Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapWorkforceSafetyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/operational/workforce-safety")
            .RequireAuthorization(WorkforceSafetyAuthorizationPolicies.Read)
            .WithTags("WorkforceSafety");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}