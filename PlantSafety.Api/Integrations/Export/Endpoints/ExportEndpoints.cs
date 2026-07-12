// ============================================================================
// File: ExportEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Minimal API endpoint group stub for Integrations.Export (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating Integrations.Export to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when Integrations.Export migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Integrations.Export.Authorization;

namespace PlantSafety.Api.Integrations.Export.Endpoints;

/// <summary>Minimal API endpoint group stub for Integrations.Export.</summary>
public static class ExportEndpoints
{
    /// <summary>
    /// Maps the Integrations.Export Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapExportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/integrations/export")
            .RequireAuthorization(ExportAuthorizationPolicies.Read)
            .WithTags("Export")
            .WithOpenApi();

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}
