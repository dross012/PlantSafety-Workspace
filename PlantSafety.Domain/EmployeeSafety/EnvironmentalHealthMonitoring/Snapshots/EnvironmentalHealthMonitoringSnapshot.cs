// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EnvironmentalHealthMonitoring
//  Layer:          Domain
//  File:           EnvironmentalHealthMonitoringSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for EnvironmentalHealthMonitoring aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EnvironmentalHealthMonitoring.Snapshots;

/// <summary>Immutable snapshot of a <see cref="EnvironmentalHealthMonitoring"/> aggregate for fast rehydration.</summary>
public sealed record EnvironmentalHealthMonitoringSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public EnvironmentalHealthMonitoringId EnvironmentalHealthMonitoringId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public EnvironmentalHealthMonitoringStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}