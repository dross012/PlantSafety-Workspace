// ============================================================================
// File: ComplianceFailureEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for ComplianceFailure (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating ComplianceFailure to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when ComplianceFailure migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Analytics.Predictive.ComplianceFailure.Authorization;

namespace PlantSafety.Api.Analytics.Predictive.ComplianceFailure.Endpoints;

/// <summary>Minimal API endpoint group stub for ComplianceFailure.</summary>
public static class ComplianceFailureEndpoints
{
    /// <summary>
    /// Maps the ComplianceFailure Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapComplianceFailureEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/analytics/predictive/compliancefailure")
            .RequireAuthorization(ComplianceFailureAuthorizationPolicies.Read)
            .WithTags("ComplianceFailure");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}