// ============================================================================
// File: AnnualAuditEndpoints.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Minimal API endpoint group stub for AnnualAudit (future alternative to MVC controllers).
// ============================================================================
//
// Onboarding Notes:
// - Currently a no-op. V1 MVC controllers are the active endpoint surface.
//
// Rationale:
// - Provides a registration point for migrating AnnualAudit to Minimal API without removing the MVC controller during transition.
//
// Future Extension Seams:
// - Implement MapGet/MapPost here when AnnualAudit migrates to Minimal API.
//
// ============================================================================

using PlantSafety.Api.Compliance.RegulatoryCalendar.Deadlines.AnnualAudit.Authorization;

namespace PlantSafety.Api.Compliance.RegulatoryCalendar.Deadlines.AnnualAudit.Endpoints;

/// <summary>Minimal API endpoint group stub for AnnualAudit.</summary>
public static class AnnualAuditEndpoints
{
    /// <summary>
    /// Maps the AnnualAudit Minimal API endpoint group onto the provided route builder.
    /// Currently a no-op stub; MVC controllers in Controllers/V1 are the active surface.
    /// </summary>
    /// <param name="app">The <see cref="IEndpointRouteBuilder"/> to register routes on.</param>
    public static void MapAnnualAuditEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("api/v1/compliance/regulatory-calendar/deadlines/annual-audit")
            .RequireAuthorization(AnnualAuditAuthorizationPolicies.Read)
            .WithTags("AnnualAudit")
            .WithOpenApi();

        // Stub - no Minimal API routes active yet. MVC controllers handle all traffic.
        _ = group;
    }
}