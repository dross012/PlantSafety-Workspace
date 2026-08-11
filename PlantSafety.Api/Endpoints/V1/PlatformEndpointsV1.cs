// ============================================================================
// File: PlatformEndpointsV1.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for platform-level V1 endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. PlatformV1Controller (MVC) is the active endpoint surface.
//
// Rationale:
// - Forward-compatible hook for migrating platform endpoints to Minimal API if needed.
//
// Future Extension Seams:
// - Implement MapGet here when platform routes migrate to Minimal API.
//
// ============================================================================

namespace PlantSafety.Api.Endpoints.V1;

/// <summary>Minimal API endpoint group stub for platform-level V1 routes.</summary>
public static class PlatformEndpointsV1
{
    /// <summary>
    /// Maps the platform V1 Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers handle all traffic.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapPlatformEndpointsV1(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/platform")
            .WithTags("Platform");

        // Stub - no Minimal API routes active yet.
        _ = group;
    }
}
