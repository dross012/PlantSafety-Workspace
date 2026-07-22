// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.ErgonomicsWorkstationDesign
//  Layer:          Domain
//  File:           ErgonomicsWorkstationDesignStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real assessment lifecycle states for the ErgonomicsWorkstationDesign aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Enums;

/// <summary>Lifecycle states for the <see cref="ErgonomicsWorkstationDesign"/> aggregate root.</summary>
public enum ErgonomicsWorkstationDesignStatus
{
    /// <summary>Procedure document drafted.</summary>
    Draft = 0,

    /// <summary>Submitted and pending approval.</summary>
    PendingApproval = 1,

    /// <summary>Approved and published for use.</summary>
    Published = 2,

    /// <summary>An assessment against this procedure is scheduled.</summary>
    AssessmentScheduled = 3,

    /// <summary>Retired/superseded.</summary>
    Retired = 4,
}
