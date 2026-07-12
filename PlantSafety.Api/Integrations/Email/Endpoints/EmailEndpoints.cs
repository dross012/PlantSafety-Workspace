// ============================================================================
// File: EmailEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Minimal API endpoint group stub for Integrations.Email (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating Integrations.Email to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when Integrations.Email migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Integrations.Email.Authorization;

namespace PlantSafety.Api.Integrations.Email.Endpoints;

/// <summary>Minimal API endpoint group stub for Integrations.Email.</summary>
public static class EmailEndpoints
{
    /// <summary>
    /// Maps the Integrations.Email Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapEmailEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/integrations/email")
            .RequireAuthorization(EmailAuthorizationPolicies.Read)
            .WithTags("Email")
            .WithOpenApi();

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}
