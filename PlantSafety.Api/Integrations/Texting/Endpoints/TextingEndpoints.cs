// ============================================================================
// File: TextingEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Minimal API endpoint group stub for Integrations.Texting (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating Integrations.Texting to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when Integrations.Texting migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Integrations.Texting.Authorization;

namespace PlantSafety.Api.Integrations.Texting.Endpoints;

/// <summary>Minimal API endpoint group stub for Integrations.Texting.</summary>
public static class TextingEndpoints
{
    /// <summary>
    /// Maps the Integrations.Texting Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapTextingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/integrations/texting")
            .RequireAuthorization(TextingAuthorizationPolicies.Read)
            .WithTags("Texting");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}
