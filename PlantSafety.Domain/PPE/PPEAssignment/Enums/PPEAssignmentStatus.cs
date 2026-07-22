// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEAssignment
//  Layer:          Domain
//  File:           PPEAssignmentStatus.cs
//  Created:        2026-07-20
//  Updated:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the PPEAssignment aggregate root, modeling
//                  "PPE Assignment & Compliance" and "PPE Training & Fit Testing".
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEAssignment.Enums;

/// <summary>Lifecycle states for the <see cref="PPEAssignment"/> aggregate root.</summary>
public enum PPEAssignmentStatus
{
    /// <summary>Assignment profile has been drafted based on job role/hazard zone/exposure but PPE has not yet been issued.</summary>
    Draft = 0,

    /// <summary>PPE has been physically issued to the worker.</summary>
    Assigned = 1,

    /// <summary>Worker has completed a passing respirator/PPE fit test for the assigned equipment.</summary>
    FitTestVerified = 2,

    /// <summary>Assignment has been flagged for replacement (failed fit test, damage, or approaching expiration).</summary>
    PendingReplacement = 3,

    /// <summary>Assignment has been retired (worker reassigned, terminated, or PPE permanently returned).</summary>
    Retired = 4,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Draft"/>.</summary>
    [Obsolete("Use Draft. Retained only for additive-only compatibility with prior scaffolding.")]
    Created = Draft,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Assigned"/>.</summary>
    [Obsolete("Use Assigned. Retained only for additive-only compatibility with prior scaffolding.")]
    Active = Assigned,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Retired"/>.</summary>
    [Obsolete("Use Retired. Retained only for additive-only compatibility with prior scaffolding.")]
    Completed = Retired,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Retired"/>.</summary>
    [Obsolete("Use Retired. Retained only for additive-only compatibility with prior scaffolding.")]
    Closed = Retired,
}
