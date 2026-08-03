// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           RouteDefinitionDetailDto.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Read-model item backing the Routing Detail metadata panel. Field names match
//                  PlantSafety.UI.Modules.Routing.Models.RoutingDetailDto exactly (RoutingId/Name/
//                  Plant/Status/Version/VersionId/EffectiveFromUtc/EffectiveToUtc/Owner/
//                  LastAuditedUtc), plus an ETag for optimistic-concurrency round-tripping into
//                  the lifecycle commands. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.ProcessManagement.Routing.Models;

/// <summary>Full route detail header (metadata panel).</summary>
public sealed record RouteDefinitionDetailDto
{
    /// <summary>Route definition identifier.</summary>
    public required Guid RoutingId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Human-readable route display name.</summary>
    public required string Name { get; init; }

    /// <summary>Plant/site display name.</summary>
    public required string Plant { get; init; }

    /// <summary>Current lifecycle status.</summary>
    public required string Status { get; init; }

    /// <summary>Current version label (e.g., "v3").</summary>
    public required string Version { get; init; }

    /// <summary>Current version identifier (most recent RouteVersionSnapshot for this route).</summary>
    public Guid? VersionId { get; init; }

    /// <summary>Effective start date (UTC).</summary>
    public required DateTime EffectiveFromUtc { get; init; }

    /// <summary>Effective end date (UTC), if any.</summary>
    public DateTime? EffectiveToUtc { get; init; }

    /// <summary>Owner display name.</summary>
    public required string Owner { get; init; }

    /// <summary>UTC timestamp of the last lifecycle/audit event.</summary>
    public required DateTime LastAuditedUtc { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field.
    /// </summary>
    public string? ETag { get; init; }
}
