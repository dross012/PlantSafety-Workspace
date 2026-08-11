// ============================================================================
// File: DOTEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for DOT (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating DOT to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when DOT migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Compliance.RegulatoryCalendar.Inspections.DOT.Authorization;

namespace PlantSafety.Api.Compliance.RegulatoryCalendar.Inspections.DOT.Endpoints;

/// <summary>Minimal API endpoint group stub for DOT.</summary>
public static class DOTEndpoints
{
    /// <summary>
    /// Maps the DOT Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapDOTEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/compliance/regulatory-calendar/inspections/dot")
            .RequireAuthorization(DOTAuthorizationPolicies.Read)
            .WithTags("DOT");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}