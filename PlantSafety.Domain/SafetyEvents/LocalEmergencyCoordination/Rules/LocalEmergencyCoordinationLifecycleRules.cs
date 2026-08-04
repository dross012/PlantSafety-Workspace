// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Domain
//  File:           LocalEmergencyCoordinationLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the LocalEmergencyCoordination aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.LocalEmergencyCoordination.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.LocalEmergencyCoordination"/> aggregate.</summary>
public static class LocalEmergencyCoordinationLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(LocalEmergencyCoordinationStatus current) => current == LocalEmergencyCoordinationStatus.Created;

    /// <summary>Returns <see langword="true"/> when coordination workflows (mutual aid, joint plans, liaisons, debriefs) may be recorded while in <paramref name="current"/>.</summary>
    public static bool CanCoordinate(LocalEmergencyCoordinationStatus current)
        => current is LocalEmergencyCoordinationStatus.Created or LocalEmergencyCoordinationStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(LocalEmergencyCoordinationStatus current) => current == LocalEmergencyCoordinationStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(LocalEmergencyCoordinationStatus current)
        => current is LocalEmergencyCoordinationStatus.Completed or LocalEmergencyCoordinationStatus.Active or LocalEmergencyCoordinationStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(LocalEmergencyCoordinationStatus current) => current == LocalEmergencyCoordinationStatus.Closed;
}
