// ============================================================================
// File: ImportEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Minimal API endpoint group stub for Integrations.Import (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating Integrations.Import to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when Integrations.Import migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Integrations.Import.Authorization;

namespace PlantSafety.Api.Integrations.Import.Endpoints;

/// <summary>Minimal API endpoint group stub for Integrations.Import.</summary>
public static class ImportEndpoints
{
    /// <summary>
    /// Maps the Integrations.Import Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapImportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/integrations/import")
            .RequireAuthorization(ImportAuthorizationPolicies.Read)
            .WithTags("Import");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}
