// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.MachineProcessMonitoring
//  Layer:          Domain
//  File:           MachineProcessMonitoringLifecycleRules.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the MachineProcessMonitoring aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.MachineProcessMonitoring.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="MachineProcessMonitoring"/> aggregate.</summary>
public static class MachineProcessMonitoringLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(MachineProcessMonitoringStatus current) => current == MachineProcessMonitoringStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(MachineProcessMonitoringStatus current) => current == MachineProcessMonitoringStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(MachineProcessMonitoringStatus current) => current == MachineProcessMonitoringStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(MachineProcessMonitoringStatus current) => current == MachineProcessMonitoringStatus.Closed;
}