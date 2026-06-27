// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Domain
//  File:           ManagementOfChangeLifecycleRules.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.ManagementOfChange.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="ManagementOfChange"/> aggregate.</summary>
public static class ManagementOfChangeLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(ManagementOfChangeStatus current) => current == ManagementOfChangeStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(ManagementOfChangeStatus current) => current == ManagementOfChangeStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(ManagementOfChangeStatus current) => current == ManagementOfChangeStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(ManagementOfChangeStatus current) => current == ManagementOfChangeStatus.Closed;
}
