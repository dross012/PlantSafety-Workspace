// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           RouteDefinitionSummaryDto.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Read-model row backing the Routing Dashboard list page. Field names match
//                  PlantSafety.UI.Modules.Routing.Models.RoutingSummaryDto exactly (RoutingId/
//                  Name/Plant/Status/Version/LastUpdatedUtc/Owner) since that UI DTO's wire shape
//                  is the pre-existing contract this build-out targets. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.ProcessManagement.Routing.Models;

/// <summary>Single row in the Routing Dashboard list.</summary>
public sealed record RouteDefinitionSummaryDto
{
    /// <summary>Route definition identifier.</summary>
    public required Guid RoutingId { get; init; }

    /// <summary>Human-readable route display name.</summary>
    public required string Name { get; init; }

    /// <summary>Plant/site display name.</summary>
    public required string Plant { get; init; }

    /// <summary>Current lifecycle status (Draft/UnderReview/Active/Retired/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Current version label (e.g., "v3").</summary>
    public required string Version { get; init; }

    /// <summary>UTC timestamp this route was last audited/updated.</summary>
    public required DateTime LastUpdatedUtc { get; init; }

    /// <summary>Owner display name.</summary>
    public required string Owner { get; init; }
}
