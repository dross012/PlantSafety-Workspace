// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Domain
//  File:           CrisisManagementStatus.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the CrisisManagement aggregate root. Uses "Declared" as
//                  the initial state (rather than the generic "Created") since that is this
//                  domain's real vocabulary, confirmed by the pre-existing CrisisManagementItemDto
//                  Status doc comment (Declared/Active/Stabilized/Closed).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.CrisisManagement.Enums;

/// <summary>Lifecycle states for the CrisisManagement aggregate root.</summary>
public enum CrisisManagementStatus
{
    /// <summary>The crisis has been declared and awaits a response.</summary>
    Declared = 0,

    /// <summary>The crisis is being actively responded to (and may have been stabilized).</summary>
    Active = 1,

    /// <summary>The crisis has been resolved.</summary>
    Completed = 2,

    /// <summary>The crisis has been formally closed.</summary>
    Closed = 3,

    /// <summary>The crisis record has been archived.</summary>
    Archived = 4,
}
