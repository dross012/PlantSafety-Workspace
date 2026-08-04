// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.RestrictedZonePolicies
//  Layer:          Domain
//  File:           RestrictedZonePolicySnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for RestrictedZonePolicy aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Snapshots;

/// <summary>Immutable snapshot of a <see cref="RestrictedZonePolicy"/> aggregate for fast rehydration.</summary>
public sealed record RestrictedZonePolicySnapshot
{
    /// <summary>Aggregate identity.</summary>
    public RestrictedZonePolicyId RestrictedZonePolicyId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public RestrictedZonePolicyStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}
