// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.SafetyZones
//  Layer:          Domain
//  File:           SafetyZoneSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for SafetyZone aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Snapshots;

/// <summary>Immutable snapshot of a <see cref="SafetyZone"/> aggregate for fast rehydration.</summary>
public sealed record SafetyZoneSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public SafetyZoneId SafetyZoneId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public SafetyZoneStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
