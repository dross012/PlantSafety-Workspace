// ============================================================================
// File: ContractorEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for Contractor (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating Contractor to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when Contractor migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Compliance.Policy.Contractor.Authorization;

namespace PlantSafety.Api.Compliance.Policy.Contractor.Endpoints;

/// <summary>Minimal API endpoint group stub for Contractor.</summary>
public static class ContractorEndpoints
{
    /// <summary>
    /// Maps the Contractor Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapContractorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/compliance/policy/contractor")
            .RequireAuthorization(ContractorAuthorizationPolicies.Read)
            .WithTags("Contractor");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}