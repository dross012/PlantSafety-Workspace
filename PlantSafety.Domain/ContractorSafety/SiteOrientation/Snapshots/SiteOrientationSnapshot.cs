// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.SiteOrientation
//  Layer:          Domain
//  File:           SiteOrientationSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for SiteOrientation aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.SiteOrientation.Snapshots;

/// <summary>Immutable snapshot of a <see cref="SiteOrientation"/> aggregate for fast rehydration.</summary>
public sealed record SiteOrientationSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public SiteOrientationId SiteOrientationId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public SiteOrientationStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}