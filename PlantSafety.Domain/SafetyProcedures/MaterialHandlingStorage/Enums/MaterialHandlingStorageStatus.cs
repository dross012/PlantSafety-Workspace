// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MaterialHandlingStorage
//  Layer:          Domain
//  File:           MaterialHandlingStorageStatus.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Real document/compliance lifecycle states for the MaterialHandlingStorage aggregate
//                  root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Enums;

/// <summary>Lifecycle states for the <see cref="MaterialHandlingStorage"/> aggregate root.</summary>
public enum MaterialHandlingStorageStatus
{
    /// <summary>Procedure document drafted.</summary>
    Draft = 0,

    /// <summary>Submitted and pending approval.</summary>
    PendingApproval = 1,

    /// <summary>Approved and published; storage area under active compliance monitoring.</summary>
    Published = 2,

    /// <summary>A walkway/egress obstruction or unsafe storage condition was flagged.</summary>
    ObstructionFlagged = 3,

    /// <summary>Retired/superseded.</summary>
    Retired = 4,
}
