// ============================================================================
// File: PlatformEndpointsV2.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for platform-level V2 endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. PlatformV2Controller (MVC) is the active endpoint surface.
//
// Rationale:
// - Forward-compatible hook for migrating platform V2 endpoints to Minimal API.
//
// Future Extension Seams:
// - Implement MapGet here when platform routes migrate to Minimal API.
//
// ============================================================================

namespace PlantSafety.Api.Endpoints.V2;

/// <summary>Minimal API endpoint group stub for platform-level V2 routes.</summary>
public static class PlatformEndpointsV2
{
    /// <summary>
    /// Maps the platform V2 Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers handle all traffic.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapPlatformEndpointsV2(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v2/platform")
            .WithTags("Platform");

        // Stub - no Minimal API routes active yet.
        _ = group;
    }
}
