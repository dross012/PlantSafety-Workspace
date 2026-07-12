// ============================================================================
// File: AllInvestigationsEndpoints.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Minimal API endpoint group stub for AllInvestigations (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating AllInvestigations to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when AllInvestigations migrates to Minimal API.
//
// ============================================================================
using PlantSafety.Api.Intelligence.AllInvestigations.Authorization;

namespace PlantSafety.Api.Intelligence.AllInvestigations.Endpoints;

/// <summary>Minimal API endpoint group stub for AllInvestigations.</summary>
public static class AllInvestigationsEndpoints
{
    /// <summary>
    /// Maps the AllInvestigations Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapAllInvestigationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/intelligence/allinvestigations")
            .RequireAuthorization(AllInvestigationsAuthorizationPolicies.Read)
            .WithTags("AllInvestigations")
            .WithOpenApi();

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}