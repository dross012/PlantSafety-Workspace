// ============================================================================
// File: CoursesEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for Courses (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating Courses to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when Courses migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Operational.Training.Courses.Authorization;

namespace PlantSafety.Api.Operational.Training.Courses.Endpoints;

/// <summary>Minimal API endpoint group stub for Courses.</summary>
public static class CoursesEndpoints
{
    /// <summary>
    /// Maps the Courses Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapCoursesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/operational/training/courses")
            .RequireAuthorization(CoursesAuthorizationPolicies.Read)
            .WithTags("Courses");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}