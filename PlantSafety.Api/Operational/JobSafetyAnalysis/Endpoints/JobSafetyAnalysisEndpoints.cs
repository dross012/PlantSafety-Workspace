// ============================================================================
// File: JobSafetyAnalysisEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for JobSafetyAnalysis (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating JobSafetyAnalysis to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when JobSafetyAnalysis migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Operational.JobSafetyAnalysis.Authorization;

namespace PlantSafety.Api.Operational.JobSafetyAnalysis.Endpoints;

/// <summary>Minimal API endpoint group stub for JobSafetyAnalysis.</summary>
public static class JobSafetyAnalysisEndpoints
{
    /// <summary>
    /// Maps the JobSafetyAnalysis Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapJobSafetyAnalysisEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/operational/job-safety-analysis")
            .RequireAuthorization(JobSafetyAnalysisAuthorizationPolicies.Read)
            .WithTags("JobSafetyAnalysis");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}