// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.StopWorkEvent
//  Layer:          Domain
//  File:           StopWorkEventLifecycleRules.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the StopWorkEvent aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.StopWorkEvent.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="StopWorkEvent"/> aggregate.</summary>
public static class StopWorkEventLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(StopWorkEventStatus current) => current == StopWorkEventStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(StopWorkEventStatus current) => current == StopWorkEventStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(StopWorkEventStatus current) => current == StopWorkEventStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(StopWorkEventStatus current) => current == StopWorkEventStatus.Closed;
}