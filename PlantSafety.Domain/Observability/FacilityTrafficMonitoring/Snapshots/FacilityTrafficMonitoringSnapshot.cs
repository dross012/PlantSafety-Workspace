// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.FacilityTrafficMonitoring
//  Layer:          Domain
//  File:           FacilityTrafficMonitoringSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for FacilityTrafficMonitoring aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Snapshots;

/// <summary>Immutable snapshot of a <see cref="FacilityTrafficMonitoring"/> aggregate for fast rehydration.</summary>
public sealed record FacilityTrafficMonitoringSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public FacilityTrafficMonitoringId FacilityTrafficMonitoringId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public FacilityTrafficMonitoringStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}