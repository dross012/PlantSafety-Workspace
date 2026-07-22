// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.EnvironmentalAssetMonitoring
//  Layer:          Domain
//  File:           EnvironmentalAssetMonitoringSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for EnvironmentalAssetMonitoring aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Snapshots;

/// <summary>Immutable snapshot of a <see cref="EnvironmentalAssetMonitoring"/> aggregate for fast rehydration.</summary>
public sealed record EnvironmentalAssetMonitoringSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public EnvironmentalAssetMonitoringId EnvironmentalAssetMonitoringId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public EnvironmentalAssetMonitoringStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}