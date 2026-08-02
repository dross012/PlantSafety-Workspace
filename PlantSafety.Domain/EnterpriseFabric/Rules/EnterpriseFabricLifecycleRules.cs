// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Domain
//  File:           EnterpriseFabricLifecycleRules.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the EnterpriseFabric aggregate.
//                  Mirrors PlantSafety.Domain.Compliance.Auditing.Rules.AuditingLifecycleRules.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnterpriseFabric.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="Aggregates.EnterpriseFabric"/> aggregate.</summary>
public static class EnterpriseFabricLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(EnterpriseFabricStatus current) => current == EnterpriseFabricStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(EnterpriseFabricStatus current) => current == EnterpriseFabricStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(EnterpriseFabricStatus current) => current == EnterpriseFabricStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(EnterpriseFabricStatus current) => current == EnterpriseFabricStatus.Closed;
}
