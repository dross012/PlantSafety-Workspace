// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Domain
//  File:           PredictiveMaintenanceAlertStatus.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the PredictiveMaintenanceAlert aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PredictiveMaintenance.Enums;

/// <summary>Lifecycle states for the PredictiveMaintenanceAlert aggregate root.</summary>
public enum PredictiveMaintenanceAlertStatus
{
    /// <summary>A failure-prediction alert has been raised and awaits review.</summary>
    Raised = 0,

    /// <summary>A reviewer has triaged the alert and is assessing next steps.</summary>
    UnderReview = 1,

    /// <summary>Corrective maintenance action has been scheduled/dispatched against the alert.</summary>
    ActionScheduled = 2,

    /// <summary>The predicted failure was addressed and the alert is resolved.</summary>
    Resolved = 3,

    /// <summary>The alert was assessed and dismissed (e.g. false positive) without corrective action.</summary>
    Dismissed = 4,

    /// <summary>The alert has been formally closed after resolution or dismissal.</summary>
    Closed = 5,
}
