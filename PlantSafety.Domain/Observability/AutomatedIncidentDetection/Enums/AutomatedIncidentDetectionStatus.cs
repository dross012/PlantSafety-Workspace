// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Domain
//  File:           AutomatedIncidentDetectionStatus.cs
//  Created:        2026-07-20
//  Updated:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the AutomatedIncidentDetection aggregate root, modeling
//                  "Automated Incident Detection" (slip-and-fall, fire/smoke, unauthorized
//                  access, man-down, equipment failure, chemical spill): automated incident
//                  creation, instant notifications to supervisors, and Alerts module integration.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.AutomatedIncidentDetection.Enums;

/// <summary>Lifecycle states for the <see cref="AutomatedIncidentDetection"/> aggregate root.</summary>
public enum AutomatedIncidentDetectionStatus
{
    /// <summary>AI/sensor detection has been recorded but the supervisor has not yet been notified.</summary>
    Detected = 0,

    /// <summary>Supervisor has been instantly notified ("Instant notifications to supervisors").</summary>
    SupervisorNotified = 1,

    /// <summary>Supervisor has acknowledged the detection and is reviewing video/sensor evidence.</summary>
    Acknowledged = 2,

    /// <summary>Detection has been escalated into a formal Incident Management record.</summary>
    EscalatedToIncident = 3,

    /// <summary>Detection has been reviewed and dismissed as a false positive.</summary>
    Dismissed = 4,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Detected"/>.</summary>
    [Obsolete("Use Detected. Retained only for additive-only compatibility with prior scaffolding.")]
    Created = Detected,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="SupervisorNotified"/>.</summary>
    [Obsolete("Use SupervisorNotified. Retained only for additive-only compatibility with prior scaffolding.")]
    Active = SupervisorNotified,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="EscalatedToIncident"/>.</summary>
    [Obsolete("Use EscalatedToIncident. Retained only for additive-only compatibility with prior scaffolding.")]
    Completed = EscalatedToIncident,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Dismissed"/>.</summary>
    [Obsolete("Use Dismissed. Retained only for additive-only compatibility with prior scaffolding.")]
    Closed = Dismissed,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Dismissed"/>.</summary>
    [Obsolete("Use Dismissed. Retained only for additive-only compatibility with prior scaffolding.")]
    Archived = Dismissed,
}
