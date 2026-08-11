// ============================================================================
// File: BackupEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for Backup (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating Backup to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when Backup migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.CrossDomain.Backup.Authorization;

namespace PlantSafety.Api.CrossDomain.Backup.Endpoints;

/// <summary>Minimal API endpoint group stub for Backup.</summary>
public static class BackupEndpoints
{
    /// <summary>
    /// Maps the Backup Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapBackupEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/cross-domain/backup")
            .RequireAuthorization(BackupAuthorizationPolicies.Read)
            .WithTags("Backup");

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}