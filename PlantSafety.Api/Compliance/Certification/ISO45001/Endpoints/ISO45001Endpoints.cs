// ============================================================================
// File: ISO45001Endpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for ISO45001 (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating ISO45001 to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when ISO45001 migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Compliance.Certification.ISO45001.Authorization;

namespace PlantSafety.Api.Compliance.Certification.ISO45001.Endpoints;

/// <summary>Minimal API endpoint group stub for ISO45001.</summary>
public static class ISO45001Endpoints
{
    /// <summary>
    /// Maps the ISO45001 Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapISO45001Endpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/compliance/certification/iso45001")
            .RequireAuthorization(ISO45001AuthorizationPolicies.Read)
            .WithTags("ISO45001")
            .WithOpenApi();

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}