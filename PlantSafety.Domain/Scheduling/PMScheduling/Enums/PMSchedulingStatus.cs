// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PMScheduling
//  Layer:          Domain
//  File:           PMSchedulingStatus.cs
//  Created:        2026-07-20
//  Updated:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the PMScheduling aggregate root, modeling
//                  "Preventive Maintenance (PM) Scheduling": scheduled inspections of safety
//                  devices, tracking overdue PM tasks, and PM compliance reporting.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PMScheduling.Enums;

/// <summary>Lifecycle states for the <see cref="PMScheduling"/> aggregate root.</summary>
public enum PMSchedulingStatus
{
    /// <summary>PM task has been drafted but not yet placed on the PM calendar.</summary>
    Draft = 0,

    /// <summary>PM task has been scheduled with a due date on the PM calendar.</summary>
    Scheduled = 1,

    /// <summary>Work on the PM task is in progress.</summary>
    InProgress = 2,

    /// <summary>PM task passed its due date without completion and has been escalated.</summary>
    Overdue = 3,

    /// <summary>PM task has been completed.</summary>
    Completed = 4,

    /// <summary>PM task has been cancelled (e.g., equipment decommissioned).</summary>
    Cancelled = 5,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Draft"/>.</summary>
    [Obsolete("Use Draft. Retained only for additive-only compatibility with prior scaffolding.")]
    Created = Draft,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Scheduled"/>.</summary>
    [Obsolete("Use Scheduled. Retained only for additive-only compatibility with prior scaffolding.")]
    Active = Scheduled,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Completed"/>.</summary>
    [Obsolete("Use Completed. Retained only for additive-only compatibility with prior scaffolding.")]
    Closed = Completed,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Completed"/>.</summary>
    [Obsolete("Use Completed. Retained only for additive-only compatibility with prior scaffolding.")]
    Archived = Completed,
}
