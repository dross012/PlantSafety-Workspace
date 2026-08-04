// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteStreams
//  Layer:          Domain
//  File:           WasteStreamSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Snapshot record for WasteStream aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Enums;
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Snapshots;

/// <summary>Immutable snapshot of a WasteStream aggregate for fast rehydration.</summary>
public sealed record WasteStreamSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public WasteStreamId WasteStreamId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public WasteStreamStatus Status { get; init; }

    /// <summary>Characterization version at snapshot time.</summary>
    public int CharacterizationVersion { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
