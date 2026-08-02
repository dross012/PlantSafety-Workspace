// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Domain
//  File:           HumanFactorsLifecycleRules.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Defines permitted lifecycle transitions for the HumanFactors aggregate.
//                  Mirrors PlantSafety.Domain.Compliance.Auditing.Rules.AuditingLifecycleRules.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.Rules;

/// <summary>Defines permitted lifecycle transitions for the <see cref="HumanFactors"/> aggregate.</summary>
public static class HumanFactorsLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the aggregate may be activated from <paramref name="current"/>.</summary>
    public static bool CanActivate(HumanFactorsStatus current) => current == HumanFactorsStatus.Created;

    /// <summary>Returns <see langword="true"/> when the aggregate may be completed from <paramref name="current"/>.</summary>
    public static bool CanComplete(HumanFactorsStatus current) => current == HumanFactorsStatus.Active;

    /// <summary>Returns <see langword="true"/> when the aggregate may be closed from <paramref name="current"/>.</summary>
    public static bool CanClose(HumanFactorsStatus current) => current == HumanFactorsStatus.Completed;

    /// <summary>Returns <see langword="true"/> when the aggregate may be archived from <paramref name="current"/>.</summary>
    public static bool CanArchive(HumanFactorsStatus current) => current == HumanFactorsStatus.Closed;
}
