// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.EmergencyResponsePlanning
//  Layer:          Domain
//  File:           EmergencyResponsePlanningStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real document/drill lifecycle states for the EmergencyResponsePlanning aggregate
//                  root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Enums;

/// <summary>Lifecycle states for the <see cref="EmergencyResponsePlanning"/> aggregate root.</summary>
public enum EmergencyResponsePlanningStatus
{
    /// <summary>Plan document drafted.</summary>
    Draft = 0,

    /// <summary>Submitted and pending approval.</summary>
    PendingApproval = 1,

    /// <summary>Approved and published.</summary>
    Published = 2,

    /// <summary>A drill has been scheduled to test the plan.</summary>
    DrillScheduled = 3,

    /// <summary>Retired/superseded.</summary>
    Retired = 4,
}
