// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Domain
//  File:           PreventiveMaintenanceTaskLifecycleRules.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Guards for the PreventiveMaintenanceTask aggregate's lifecycle transitions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PreventiveMaintenance.Rules;

/// <summary>Defines permitted lifecycle transitions for the PreventiveMaintenanceTask aggregate.</summary>
public static class PreventiveMaintenanceTaskLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when work may start from <paramref name="current"/>.</summary>
    public static bool CanStart(PreventiveMaintenanceTaskStatus current) => current == PreventiveMaintenanceTaskStatus.Scheduled;

    /// <summary>Returns <see langword="true"/> when the task may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(PreventiveMaintenanceTaskStatus current) => current == PreventiveMaintenanceTaskStatus.InProgress;

    /// <summary>Returns <see langword="true"/> when the task may be verified from <paramref name="current"/>.</summary>
    public static bool CanVerify(PreventiveMaintenanceTaskStatus current) => current == PreventiveMaintenanceTaskStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the task may be cancelled from <paramref name="current"/>.</summary>
    public static bool CanCancel(PreventiveMaintenanceTaskStatus current) =>
        current == PreventiveMaintenanceTaskStatus.Scheduled || current == PreventiveMaintenanceTaskStatus.InProgress;

    /// <summary>Returns <see langword="true"/> when the task may be marked overdue from <paramref name="current"/>.</summary>
    public static bool CanMarkOverdue(PreventiveMaintenanceTaskStatus current) => current == PreventiveMaintenanceTaskStatus.Scheduled;
}
