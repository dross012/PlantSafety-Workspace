// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.HazardIsolationSequencing
//  Layer:          Domain
//  File:           HazardIsolationSequencingLifecycleRules.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the HazardIsolationSequencing aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.HazardIsolationSequencing.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="HazardIsolationSequencing"/> aggregate.</summary>
public static class HazardIsolationSequencingLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(HazardIsolationSequencingStatus current) => current == HazardIsolationSequencingStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(HazardIsolationSequencingStatus current) => current == HazardIsolationSequencingStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(HazardIsolationSequencingStatus current) => current == HazardIsolationSequencingStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(HazardIsolationSequencingStatus current) => current == HazardIsolationSequencingStatus.Closed;
}