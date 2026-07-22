// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEZoneMapping
//  Layer:          Domain
//  File:           PPEZoneMappingSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for PPEZoneMapping aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEZoneMapping.Snapshots;

/// <summary>Immutable snapshot of a <see cref="PPEZoneMapping"/> aggregate for fast rehydration.</summary>
public sealed record PPEZoneMappingSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public PPEZoneMappingId PPEZoneMappingId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public PPEZoneMappingStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}