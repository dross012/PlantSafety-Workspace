// ============================================================================
// File: EPAEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for EPA (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating EPA to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when EPA migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Compliance.RegulatoryCalendar.Deadlines.EPA.Authorization;

namespace PlantSafety.Api.Compliance.RegulatoryCalendar.Deadlines.EPA.Endpoints;

/// <summary>Minimal API endpoint group stub for EPA.</summary>
public static class EPAEndpoints
{
    /// <summary>
    /// Maps the EPA Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapEPAEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/compliance/regulatory-calendar/deadlines/epa")
            .RequireAuthorization(EPAAuthorizationPolicies.Read)
            .WithTags("EPA");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}