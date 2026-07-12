// ============================================================================
// File: DataLakeEndpoints.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Minimal API endpoint group stub for DataLake (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating DataLake to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when DataLake migrates to Minimal API.
//
// ============================================================================
using PlantSafety.Api.Intelligence.DataLake.Authorization;

namespace PlantSafety.Api.Intelligence.DataLake.Endpoints;

/// <summary>Minimal API endpoint group stub for DataLake.</summary>
public static class DataLakeEndpoints
{
    /// <summary>
    /// Maps the DataLake Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapDataLakeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/intelligence/datalake")
            .RequireAuthorization(DataLakeAuthorizationPolicies.Read)
            .WithTags("DataLake")
            .WithOpenApi();

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}