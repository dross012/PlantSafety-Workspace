// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Domain
//  File:           HumanFactorsStatus.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the HumanFactors aggregate root. Phase 4 full build-out
//                  (2026-08-02): this module previously had no lifecycle at all. Mirrors the
//                  generic Created/Active/Completed/Closed/Archived shape used across every
//                  other module in this codebase (e.g. Compliance.Auditing.Enums.AuditingStatus).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.Enums;

/// <summary>Lifecycle states for the <see cref="HumanFactors"/> aggregate root.</summary>
public enum HumanFactorsStatus
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
