// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.WorkOrderPriority
//  Layer:          Domain
//  File:           WorkOrderPriorityStatus.cs
//  Created:        2026-07-20
//  Updated:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the WorkOrderPriority aggregate root, modeling
//                  "Work Order Priorities (P1-P4)": priority-based scheduling engine, work order
//                  aging reports, and automatic escalation for overdue P1/P2 items.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.WorkOrderPriority.Enums;

/// <summary>Lifecycle states for the <see cref="WorkOrderPriority"/> aggregate root.</summary>
public enum WorkOrderPriorityStatus
{
    /// <summary>Work order has been opened and prioritized (P1-P4) but work has not started.</summary>
    Open = 0,

    /// <summary>Work is in progress against the prioritized work order.</summary>
    InProgress = 1,

    /// <summary>Work order passed its priority-based due window unresolved and was automatically escalated (P1/P2 only).</summary>
    Escalated = 2,

    /// <summary>Hazard addressed by the work order has been resolved.</summary>
    Resolved = 3,

    /// <summary>Work order has been closed with no further action required.</summary>
    Closed = 4,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Open"/>.</summary>
    [Obsolete("Use Open. Retained only for additive-only compatibility with prior scaffolding.")]
    Created = Open,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="InProgress"/>.</summary>
    [Obsolete("Use InProgress. Retained only for additive-only compatibility with prior scaffolding.")]
    Active = InProgress,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Resolved"/>.</summary>
    [Obsolete("Use Resolved. Retained only for additive-only compatibility with prior scaffolding.")]
    Completed = Resolved,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Closed"/>.</summary>
    [Obsolete("Use Closed. Retained only for additive-only compatibility with prior scaffolding.")]
    Archived = Closed,
}
