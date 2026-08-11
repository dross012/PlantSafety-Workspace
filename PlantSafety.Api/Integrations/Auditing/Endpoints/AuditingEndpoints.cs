// ============================================================================
// File: AuditingEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Minimal API endpoint group stub for Integrations.Auditing (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating Integrations.Auditing to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when Integrations.Auditing migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Integrations.Auditing.Authorization;

namespace PlantSafety.Api.Integrations.Auditing.Endpoints;

/// <summary>Minimal API endpoint group stub for Integrations.Auditing.</summary>
public static class AuditingEndpoints
{
    /// <summary>
    /// Maps the Integrations.Auditing Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapAuditingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/integrations/auditing")
            .RequireAuthorization(AuditingAuthorizationPolicies.Read)
            .WithTags("Auditing");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}
