// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Domain
//  File:           PredictiveMaintenanceAlertLifecycleRules.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Guards for the PredictiveMaintenanceAlert aggregate's lifecycle transitions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PredictiveMaintenance.Rules;

/// <summary>Defines permitted lifecycle transitions for the PredictiveMaintenanceAlert aggregate.</summary>
public static class PredictiveMaintenanceAlertLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the alert may be reviewed from <paramref name="current"/>.</summary>
    public static bool CanReview(PredictiveMaintenanceAlertStatus current) => current == PredictiveMaintenanceAlertStatus.Raised;

    /// <summary>Returns <see langword="true"/> when corrective action may be scheduled from <paramref name="current"/>.</summary>
    public static bool CanScheduleAction(PredictiveMaintenanceAlertStatus current) => current == PredictiveMaintenanceAlertStatus.UnderReview;

    /// <summary>Returns <see langword="true"/> when the alert may be resolved from <paramref name="current"/>.</summary>
    public static bool CanResolve(PredictiveMaintenanceAlertStatus current) => current == PredictiveMaintenanceAlertStatus.ActionScheduled;

    /// <summary>Returns <see langword="true"/> when the alert may be dismissed from <paramref name="current"/>.</summary>
    public static bool CanDismiss(PredictiveMaintenanceAlertStatus current) =>
        current == PredictiveMaintenanceAlertStatus.Raised || current == PredictiveMaintenanceAlertStatus.UnderReview;

    /// <summary>Returns <see langword="true"/> when the alert may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(PredictiveMaintenanceAlertStatus current) =>
        current == PredictiveMaintenanceAlertStatus.Resolved || current == PredictiveMaintenanceAlertStatus.Dismissed;
}
