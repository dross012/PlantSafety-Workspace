// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EmergencyPreparedness
//  Layer:          Domain
//  File:           EmergencyPreparednessStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real drill/readiness lifecycle states for the EmergencyPreparedness aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Enums;

/// <summary>Lifecycle states for the <see cref="EmergencyPreparedness"/> aggregate root.</summary>
public enum EmergencyPreparednessStatus
{
    /// <summary>Emergency response plan documented.</summary>
    Documented = 0,

    /// <summary>A drill has been scheduled to test the plan.</summary>
    DrillScheduled = 1,

    /// <summary>A drill has been conducted and results recorded.</summary>
    DrillConducted = 2,

    /// <summary>Readiness verified at or above the acceptable threshold.</summary>
    ReadinessVerified = 3,

    /// <summary>A readiness gap was identified (drill exposed a deficiency).</summary>
    ReadinessGapIdentified = 4,
}
