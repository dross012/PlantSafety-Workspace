// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.MachineProcessMonitoring
//  Layer:          Domain
//  File:           MachineProcessMonitoringStatus.cs
//  Created:        2026-07-20
//  Updated:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the MachineProcessMonitoring aggregate root, modeling
//                  "Machine & Process Safety Monitoring": LOTO state monitoring, robotic cell
//                  activity, unexpected startups, bypassed guards/disabled interlocks, and
//                  real-time hazard alerts.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.MachineProcessMonitoring.Enums;

/// <summary>Lifecycle states for the <see cref="MachineProcessMonitoring"/> aggregate root.</summary>
public enum MachineProcessMonitoringStatus
{
    /// <summary>Sensor reading is within the normal operating range.</summary>
    Normal = 0,

    /// <summary>Most recent sensor reading breached its configured safety threshold ("real-time hazard alerts").</summary>
    ThresholdBreached = 1,

    /// <summary>AI/ML process anomaly has been flagged (e.g., unexpected startup, bypassed guard, disabled interlock).</summary>
    AnomalyFlagged = 2,

    /// <summary>An operator or supervisor has acknowledged the breach/anomaly and is investigating.</summary>
    Acknowledged = 3,

    /// <summary>The breach/anomaly has been resolved and the sensor channel has returned to normal.</summary>
    Resolved = 4,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Normal"/>.</summary>
    [Obsolete("Use Normal. Retained only for additive-only compatibility with prior scaffolding.")]
    Created = Normal,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="ThresholdBreached"/>.</summary>
    [Obsolete("Use ThresholdBreached. Retained only for additive-only compatibility with prior scaffolding.")]
    Active = ThresholdBreached,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Resolved"/>.</summary>
    [Obsolete("Use Resolved. Retained only for additive-only compatibility with prior scaffolding.")]
    Completed = Resolved,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Resolved"/>.</summary>
    [Obsolete("Use Resolved. Retained only for additive-only compatibility with prior scaffolding.")]
    Closed = Resolved,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Resolved"/>.</summary>
    [Obsolete("Use Resolved. Retained only for additive-only compatibility with prior scaffolding.")]
    Archived = Resolved,
}
