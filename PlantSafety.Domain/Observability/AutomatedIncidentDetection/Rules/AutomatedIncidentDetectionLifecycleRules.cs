// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Domain
//  File:           AutomatedIncidentDetectionLifecycleRules.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the AutomatedIncidentDetection aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.AutomatedIncidentDetection.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="AutomatedIncidentDetection"/> aggregate.</summary>
public static class AutomatedIncidentDetectionLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(AutomatedIncidentDetectionStatus current) => current == AutomatedIncidentDetectionStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(AutomatedIncidentDetectionStatus current) => current == AutomatedIncidentDetectionStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(AutomatedIncidentDetectionStatus current) => current == AutomatedIncidentDetectionStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(AutomatedIncidentDetectionStatus current) => current == AutomatedIncidentDetectionStatus.Closed;
}