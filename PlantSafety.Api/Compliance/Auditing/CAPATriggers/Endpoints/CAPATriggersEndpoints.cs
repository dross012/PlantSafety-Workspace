// ============================================================================
// File: CAPATriggersEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for CAPATriggers (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating CAPATriggers to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when CAPATriggers migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Compliance.Auditing.CAPATriggers.Authorization;

namespace PlantSafety.Api.Compliance.Auditing.CAPATriggers.Endpoints;

/// <summary>Minimal API endpoint group stub for CAPATriggers.</summary>
public static class CAPATriggersEndpoints
{
    /// <summary>
    /// Maps the CAPATriggers Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapCAPATriggersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/compliance/auditing/capatriggers")
            .RequireAuthorization(CAPATriggersAuthorizationPolicies.Read)
            .WithTags("CAPATriggers");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}