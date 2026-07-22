// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.PresenceSensingDevices
//  Layer:          Domain
//  File:           PresenceSensingDevicesSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for PresenceSensingDevices aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.PresenceSensingDevices.Snapshots;

/// <summary>Immutable snapshot of a <see cref="PresenceSensingDevices"/> aggregate for fast rehydration.</summary>
public sealed record PresenceSensingDevicesSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public PresenceSensingDevicesId PresenceSensingDevicesId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public PresenceSensingDevicesStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}