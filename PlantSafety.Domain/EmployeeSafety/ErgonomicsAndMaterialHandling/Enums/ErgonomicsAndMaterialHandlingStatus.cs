// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.ErgonomicsAndMaterialHandling
//  Layer:          Domain
//  File:           ErgonomicsAndMaterialHandlingStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real assessment lifecycle states for the ErgonomicsAndMaterialHandling aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Enums;

/// <summary>Lifecycle states for the <see cref="ErgonomicsAndMaterialHandling"/> aggregate root.</summary>
public enum ErgonomicsAndMaterialHandlingStatus
{
    /// <summary>Ergonomic assessment scheduled for a workstation/task.</summary>
    AssessmentScheduled = 0,

    /// <summary>Assessment completed and risk factors recorded.</summary>
    Assessed = 1,

    /// <summary>Engineering/administrative controls (lifting aids, job rotation, layout changes) implemented.</summary>
    ControlsImplemented = 2,

    /// <summary>A follow-up assessment has been scheduled to verify control effectiveness.</summary>
    FollowUpScheduled = 3,

    /// <summary>Assessment closed: risk mitigated to an acceptable level.</summary>
    Closed = 4,
}
