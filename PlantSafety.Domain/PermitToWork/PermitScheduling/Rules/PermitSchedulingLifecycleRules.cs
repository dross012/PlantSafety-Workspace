// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitScheduling
//  Layer:          Domain
//  File:           PermitSchedulingLifecycleRules.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the PermitScheduling aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitScheduling.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="PermitScheduling"/> aggregate.</summary>
public static class PermitSchedulingLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(PermitSchedulingStatus current) => current == PermitSchedulingStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(PermitSchedulingStatus current) => current == PermitSchedulingStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(PermitSchedulingStatus current) => current == PermitSchedulingStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(PermitSchedulingStatus current) => current == PermitSchedulingStatus.Closed;
}