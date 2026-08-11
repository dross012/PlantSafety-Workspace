// ============================================================================
// File: EmployeesEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for Employees (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating Employees to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when Employees migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.CrossDomain.Employees.Authorization;

namespace PlantSafety.Api.CrossDomain.Employees.Endpoints;

/// <summary>Minimal API endpoint group stub for Employees.</summary>
public static class EmployeesEndpoints
{
    /// <summary>
    /// Maps the Employees Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapEmployeesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/cross-domain/employees")
            .RequireAuthorization(EmployeesAuthorizationPolicies.Read)
            .WithTags("Employees");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}