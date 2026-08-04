// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.Manifests
//  Layer:          Domain
//  File:           HazardousWasteManifestSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Snapshot record for HazardousWasteManifest aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.Manifests.Enums;
using PlantSafety.Domain.HazardousWasteManagement.Manifests.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.Manifests.Snapshots;

/// <summary>Immutable snapshot of a HazardousWasteManifest aggregate for fast rehydration.</summary>
public sealed record HazardousWasteManifestSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public HazardousWasteManifestId HazardousWasteManifestId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public HazardousWasteManifestStatus Status { get; init; }

    /// <summary><see langword="true"/> when the manifest was immutable at snapshot time.</summary>
    public bool IsImmutable { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
