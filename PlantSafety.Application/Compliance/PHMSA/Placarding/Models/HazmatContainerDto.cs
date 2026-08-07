// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Application
//  File:           HazmatContainerDto.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single HazmatContainer record. Backs both
//                  GetHazmatContainerQuery and ListHazmatContainersQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto's shape.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.PHMSA.Placarding.Models;

/// <summary>A single HazmatContainer read-model record.</summary>
public sealed record HazmatContainerDto
{
    /// <summary>Record identifier.</summary>
    public required Guid HazmatContainerId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Draft/MarkedAndLabeled/PlacardingCalculated/Retired).</summary>
    public required string Status { get; init; }

    /// <summary>Physical container/packaging form factor.</summary>
    public required string ContainerType { get; init; }

    /// <summary>Reference to the classified material inside this container (Classification module).</summary>
    public required Guid MaterialId { get; init; }

    /// <summary>Reference to the authorized packaging specification (Packaging module).</summary>
    public required Guid PackagingSpecId { get; init; }

    /// <summary>Text markings applied.</summary>
    public IReadOnlyList<string> Markings { get; init; } = [];

    /// <summary>Hazard label names.</summary>
    public IReadOnlyList<string> Labels { get; init; } = [];

    /// <summary>Placard names required if this container/load meets bulk thresholds.</summary>
    public IReadOnlyList<string> Placards { get; init; } = [];

    /// <summary>Whether orientation arrows are required on this container's packaging.</summary>
    public bool OrientationArrowsRequired { get; init; }

    /// <summary>Explosives compatibility group letter (A-N, S). Only meaningful for Class 1 materials.</summary>
    public string? CompatibilityGroup { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}
