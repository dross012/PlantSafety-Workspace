// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Domain
//  File:           OperationalSafetyLifecycleRules.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the OperationalSafety aggregate.
//                  Mirrors PlantSafety.Domain.Compliance.Auditing.Rules.AuditingLifecycleRules.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.OperationalSafety.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="OperationalSafety"/> aggregate.</summary>
public static class OperationalSafetyLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(OperationalSafetyStatus current) => current == OperationalSafetyStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(OperationalSafetyStatus current) => current == OperationalSafetyStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(OperationalSafetyStatus current) => current == OperationalSafetyStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(OperationalSafetyStatus current) => current == OperationalSafetyStatus.Closed;
}
