// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit
//  Layer:          Domain
//  File:           DischargeLimitSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for DischargeLimit aggregate rehydration. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Snapshots;

/// <summary>Immutable snapshot of a <see cref="DischargeLimit"/> aggregate for fast rehydration.</summary>
public sealed record DischargeLimitSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public DischargeLimitId DischargeLimitId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public DischargeLimitStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}