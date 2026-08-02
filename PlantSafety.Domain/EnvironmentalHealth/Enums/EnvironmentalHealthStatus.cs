// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Domain
//  File:           EnvironmentalHealthStatus.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the EnvironmentalHealth aggregate root. Phase 4 full
//                  build-out (2026-08-02): this aggregate previously had no lifecycle field at
//                  all -- its pre-existing "Status" property (renamed to EvaluatedStatus by this
//                  same build-out) was a free-text business classification
//                  (Normal/Warning/Critical), not a lifecycle state, so a distinct enum + name is
//                  introduced here to avoid the collision. Mirrors the generic
//                  Created/Active/Completed/Closed/Archived shape used across every other module
//                  in this codebase (e.g. Compliance.Auditing.Enums.AuditingStatus). Distinct
//                  bounded context from the live Features.EnvironmentalHealth "reading" vertical
//                  and from EmployeeSafety.EnvironmentalHealthMonitoring. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.Enums;

/// <summary>Lifecycle states for the <see cref="EnvironmentalHealth"/> aggregate root.</summary>
public enum EnvironmentalHealthStatus
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
