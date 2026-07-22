// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.MachineProcessMonitoring
//  Layer:          Domain
//  File:           MachineProcessMonitoringSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for MachineProcessMonitoring aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.MachineProcessMonitoring.Snapshots;

/// <summary>Immutable snapshot of a <see cref="MachineProcessMonitoring"/> aggregate for fast rehydration.</summary>
public sealed record MachineProcessMonitoringSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public MachineProcessMonitoringId MachineProcessMonitoringId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public MachineProcessMonitoringStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}