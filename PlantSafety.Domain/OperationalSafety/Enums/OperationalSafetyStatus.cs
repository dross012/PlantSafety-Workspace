// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Domain
//  File:           OperationalSafetyStatus.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the OperationalSafety aggregate root. Phase 4 full
//                  build-out (2026-08-02): this module previously had no lifecycle at all.
//                  Mirrors the generic Created/Active/Completed/Closed/Archived shape used
//                  across every other module in this codebase (e.g.
//                  Compliance.Auditing.Enums.AuditingStatus). Distinct from the pre-existing,
//                  unrelated OperationalSafety.Alerts/Checklists/PPEManagement/RiskManagement/
//                  Schedules Infrastructure interfaces. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.OperationalSafety.Enums;

/// <summary>Lifecycle states for the <see cref="OperationalSafety"/> aggregate root.</summary>
public enum OperationalSafetyStatus
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
