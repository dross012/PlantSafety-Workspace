// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Domain
//  File:           EnterpriseFabricStatus.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the EnterpriseFabric aggregate root. Phase 4 full
//                  build-out -- this aggregate previously had no lifecycle at all (identity-only
//                  scaffold). Mirrors the generic Created/Active/Completed/Closed/Archived shape
//                  used throughout this effort (e.g. PlantSafety.Domain.Compliance.Auditing
//                  .Enums.AuditingStatus).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnterpriseFabric.Enums;

/// <summary>Lifecycle states for the <see cref="Aggregates.EnterpriseFabric"/> aggregate root.</summary>
public enum EnterpriseFabricStatus
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
