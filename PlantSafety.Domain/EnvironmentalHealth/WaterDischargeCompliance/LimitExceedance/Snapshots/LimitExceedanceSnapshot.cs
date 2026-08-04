// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance
//  Layer:          Domain
//  File:           LimitExceedanceSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for LimitExceedance aggregate rehydration. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Snapshots;

/// <summary>Immutable snapshot of a <see cref="LimitExceedance"/> aggregate for fast rehydration.</summary>
public sealed record LimitExceedanceSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public LimitExceedanceId LimitExceedanceId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public LimitExceedanceStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}