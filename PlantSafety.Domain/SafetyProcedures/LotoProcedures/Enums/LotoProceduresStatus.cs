// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.LotoProcedures
//  Layer:          Domain
//  File:           LotoProceduresStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real document lifecycle states for the LotoProcedures aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.LotoProcedures.Enums;

/// <summary>Lifecycle states for the <see cref="LotoProcedures"/> aggregate root.</summary>
public enum LotoProceduresStatus
{
    /// <summary>Procedure document drafted.</summary>
    Draft = 0,

    /// <summary>Submitted and pending approval.</summary>
    PendingApproval = 1,

    /// <summary>Approved and published for use.</summary>
    Published = 2,

    /// <summary>Flagged non-compliant during the mandatory annual audit.</summary>
    AuditFlagged = 3,

    /// <summary>Under revision.</summary>
    UnderRevision = 4,

    /// <summary>Retired/superseded.</summary>
    Retired = 5,
}
