// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.EmergencyDrillPlanning
//  Layer:          Domain
//  File:           EmergencyDrillPlanningSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for EmergencyDrillPlanning aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.EmergencyDrillPlanning.Snapshots;

/// <summary>Immutable snapshot of a <see cref="EmergencyDrillPlanning"/> aggregate for fast rehydration.</summary>
public sealed record EmergencyDrillPlanningSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public EmergencyDrillPlanningId EmergencyDrillPlanningId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public EmergencyDrillPlanningStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}