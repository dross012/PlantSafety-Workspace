// ============================================================================
// File: OSHAEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for OSHA (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating OSHA to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when OSHA migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Compliance.RegulatoryCalendar.Inspections.OSHA.Authorization;

namespace PlantSafety.Api.Compliance.RegulatoryCalendar.Inspections.OSHA.Endpoints;

/// <summary>Minimal API endpoint group stub for OSHA.</summary>
public static class OSHAEndpoints
{
    /// <summary>
    /// Maps the OSHA Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapOSHAEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/compliance/regulatory-calendar/inspections/osha")
            .RequireAuthorization(OSHAAuthorizationPolicies.Read)
            .WithTags("OSHA");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}