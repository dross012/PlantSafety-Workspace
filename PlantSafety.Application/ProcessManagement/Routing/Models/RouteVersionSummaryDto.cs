// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           RouteVersionSummaryDto.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Read-model row backing the Routing Version History timeline. Shape matches
//                  PlantSafety.UI.Modules.Routing.Models.RoutingVersionDto field-for-field.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.ProcessManagement.Routing.Models;

/// <summary>Single version history timeline node.</summary>
public sealed record RouteVersionSummaryDto
{
    /// <summary>Version snapshot identifier.</summary>
    public required Guid VersionId { get; init; }

    /// <summary>Version label (e.g., "v3").</summary>
    public required string VersionLabel { get; init; }

    /// <summary>Effective date (UTC).</summary>
    public required DateTime EffectiveDateUtc { get; init; }

    /// <summary>Version status (Approved/Pending/Rejected).</summary>
    public required string Status { get; init; }
}
