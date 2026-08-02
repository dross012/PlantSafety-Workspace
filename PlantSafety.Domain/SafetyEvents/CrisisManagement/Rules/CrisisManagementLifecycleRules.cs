// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Domain
//  File:           CrisisManagementLifecycleRules.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Guards for the CrisisManagement aggregate's lifecycle transitions.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.CrisisManagement.Rules;

/// <summary>Defines permitted lifecycle transitions for the CrisisManagement aggregate.</summary>
public static class CrisisManagementLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the crisis may be responded to from <paramref name="current"/>.</summary>
    public static bool CanRespondTo(CrisisManagementStatus current) => current == CrisisManagementStatus.Declared;

    /// <summary>Returns <see langword="true"/> when the crisis may be stabilized from <paramref name="current"/>.</summary>
    public static bool CanStabilize(CrisisManagementStatus current) => current == CrisisManagementStatus.Active;

    /// <summary>Returns <see langword="true"/> when the crisis may be resolved from <paramref name="current"/>.</summary>
    public static bool CanResolve(CrisisManagementStatus current) => current == CrisisManagementStatus.Active;

    /// <summary>Returns <see langword="true"/> when the crisis may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(CrisisManagementStatus current) => current == CrisisManagementStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the crisis may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(CrisisManagementStatus current) => current == CrisisManagementStatus.Closed;
}
