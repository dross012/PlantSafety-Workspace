// ============================================================================
// File: IncidentPredictionEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for IncidentPrediction (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating IncidentPrediction to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when IncidentPrediction migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Analytics.Predictive.IncidentPrediction.Authorization;

namespace PlantSafety.Api.Analytics.Predictive.IncidentPrediction.Endpoints;

/// <summary>Minimal API endpoint group stub for IncidentPrediction.</summary>
public static class IncidentPredictionEndpoints
{
    /// <summary>
    /// Maps the IncidentPrediction Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapIncidentPredictionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/analytics/predictive/incidentprediction")
            .RequireAuthorization(IncidentPredictionAuthorizationPolicies.Read)
            .WithTags("IncidentPrediction");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}