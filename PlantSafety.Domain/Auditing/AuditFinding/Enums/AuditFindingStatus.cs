// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditFinding
//  Layer:          Domain
//  File:           AuditFindingStatus.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the AuditFinding aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditFinding.Enums;

/// <summary>Lifecycle states for the <see cref="AuditFinding"/> aggregate root.</summary>
public enum AuditFindingStatus
{
    /// <summary>Record has been created and awaits processing.</summary>
    Created = 0,

    /// <summary>Record is active and under management.</summary>
    Active = 1,

    /// <summary>Record has been completed or resolved.</summary>
    Completed = 2,

    /// <summary>Record has been closed with no further action required.</summary>
    Closed = 3,

    /// <summary>Record has been archived for long-term retention.</summary>
    Archived = 4,
}