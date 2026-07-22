// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.EmergencyResponsePlanning
//  Layer:          Domain
//  File:           EmergencyResponsePlanningSnapshot.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Snapshot record for EmergencyResponsePlanning aggregate rehydration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Snapshots;

/// <summary>Immutable snapshot of a <see cref="EmergencyResponsePlanning"/> aggregate for fast rehydration.</summary>
public sealed record EmergencyResponsePlanningSnapshot
{
    /// <summary>Aggregate identity.</summary>
    public EmergencyResponsePlanningId EmergencyResponsePlanningId { get; init; }

    /// <summary>Lifecycle status at snapshot time.</summary>
    public EmergencyResponsePlanningStatus Status { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; init; }
}