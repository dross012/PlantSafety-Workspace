// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Domain
//  File:           EnvironmentalHealthLifecycleRules.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the EnvironmentalHealth
//                  aggregate. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Rules.AuditingLifecycleRules.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="EnvironmentalHealth"/> aggregate.</summary>
public static class EnvironmentalHealthLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(EnvironmentalHealthStatus current) => current == EnvironmentalHealthStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(EnvironmentalHealthStatus current) => current == EnvironmentalHealthStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(EnvironmentalHealthStatus current) => current == EnvironmentalHealthStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(EnvironmentalHealthStatus current) => current == EnvironmentalHealthStatus.Closed;
}
