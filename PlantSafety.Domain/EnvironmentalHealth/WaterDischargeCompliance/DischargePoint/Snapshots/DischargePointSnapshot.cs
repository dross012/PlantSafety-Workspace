// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePoint
//  Layer:          Domain
//  File:           DischargePointSnapshot.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for DischargePoint aggregate rehydration. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Snapshots;

/// <summary>Immutable snapshot of a <see cref="DischargePoint"/> aggregate for fast rehydration.</summary>
public sealed record DischargePointSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public DischargePointId DischargePointId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public DischargePointStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}