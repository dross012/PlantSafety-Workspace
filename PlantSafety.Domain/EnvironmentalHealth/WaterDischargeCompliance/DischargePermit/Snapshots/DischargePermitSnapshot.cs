// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePermit
//  Layer:          Domain
//  File:           DischargePermitSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for DischargePermit aggregate rehydration. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Snapshots;

/// <summary>Immutable snapshot of a <see cref="DischargePermit"/> aggregate for fast rehydration.</summary>
public sealed record DischargePermitSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public DischargePermitId DischargePermitId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public DischargePermitStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}