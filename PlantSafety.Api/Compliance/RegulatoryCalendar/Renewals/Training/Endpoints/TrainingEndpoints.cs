// ============================================================================
// File: TrainingEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for Training (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating Training to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when Training migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Compliance.RegulatoryCalendar.Renewals.Training.Authorization;

namespace PlantSafety.Api.Compliance.RegulatoryCalendar.Renewals.Training.Endpoints;

/// <summary>Minimal API endpoint group stub for Training.</summary>
public static class TrainingEndpoints
{
    /// <summary>
    /// Maps the Training Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapTrainingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/compliance/regulatory-calendar/renewals/training")
            .RequireAuthorization(TrainingAuthorizationPolicies.Read)
            .WithTags("Training")
            .WithOpenApi();

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}