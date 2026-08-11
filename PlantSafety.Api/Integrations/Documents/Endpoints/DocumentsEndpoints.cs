// ============================================================================
// File: DocumentsEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Minimal API endpoint group stub for Integrations.Documents (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating Integrations.Documents to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when Integrations.Documents migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Integrations.Documents.Authorization;

namespace PlantSafety.Api.Integrations.Documents.Endpoints;

/// <summary>Minimal API endpoint group stub for Integrations.Documents.</summary>
public static class DocumentsEndpoints
{
    /// <summary>
    /// Maps the Integrations.Documents Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapDocumentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/integrations/documents")
            .RequireAuthorization(DocumentsAuthorizationPolicies.Read)
            .WithTags("Documents");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}
