// ============================================================================
// File: CultureIndexEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for CultureIndex (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating CultureIndex to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when CultureIndex migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Analytics.Culture.CultureIndex.Authorization;

namespace PlantSafety.Api.Analytics.Culture.CultureIndex.Endpoints;

/// <summary>Minimal API endpoint group stub for CultureIndex.</summary>
public static class CultureIndexEndpoints
{
    /// <summary>
    /// Maps the CultureIndex Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapCultureIndexEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/analytics/culture/cultureindex")
            .RequireAuthorization(CultureIndexAuthorizationPolicies.Read)
            .WithTags("CultureIndex");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}