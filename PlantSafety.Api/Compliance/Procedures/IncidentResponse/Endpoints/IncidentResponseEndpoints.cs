// ============================================================================
// File: IncidentResponseEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for IncidentResponse (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating IncidentResponse to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when IncidentResponse migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Compliance.Procedures.IncidentResponse.Authorization;

namespace PlantSafety.Api.Compliance.Procedures.IncidentResponse.Endpoints;

/// <summary>Minimal API endpoint group stub for IncidentResponse.</summary>
public static class IncidentResponseEndpoints
{
    /// <summary>
    /// Maps the IncidentResponse Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapIncidentResponseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/compliance/procedures/incidentresponse")
            .RequireAuthorization(IncidentResponseAuthorizationPolicies.Read)
            .WithTags("IncidentResponse")
            .WithOpenApi();

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}