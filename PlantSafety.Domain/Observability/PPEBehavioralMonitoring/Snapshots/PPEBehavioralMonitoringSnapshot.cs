// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.PPEBehavioralMonitoring
//  Layer:          Domain
//  File:           PPEBehavioralMonitoringSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for PPEBehavioralMonitoring aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Snapshots;

/// <summary>Immutable snapshot of a <see cref="PPEBehavioralMonitoring"/> aggregate for fast rehydration.</summary>
public sealed record PPEBehavioralMonitoringSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public PPEBehavioralMonitoringId PPEBehavioralMonitoringId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public PPEBehavioralMonitoringStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}