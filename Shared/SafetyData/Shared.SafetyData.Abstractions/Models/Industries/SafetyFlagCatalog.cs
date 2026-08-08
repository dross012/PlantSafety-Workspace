// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries
//  Layer:          Abstractions
//  File:           SafetyFlagCatalog.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Fixed, ordered enumeration of every individual SafetyFlag
//                  bit (declaration order) plus a deterministic decomposition
//                  helper. Shared by every Industry-subsystem consumer that
//                  needs to iterate the single flags set on an
//                  IndustryDefinition.Flags bitmask (Hazard mappings, Required
//                  Safety Modules, SOP templates, Audit checklists, Event
//                  routing rules, Compliance profiles). Introduces no new
//                  flags -- only enumerates the existing SafetyFlag members.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries;

/// <summary>
/// Deterministic enumeration and decomposition helpers for <see cref="SafetyFlag"/>.
/// </summary>
public static class SafetyFlagCatalog
{
    /// <summary>Every individual (single-bit) <see cref="SafetyFlag"/> value, in declaration order.</summary>
    public static IReadOnlyList<SafetyFlag> AllFlags { get; } = new[]
    {
        SafetyFlag.Machinery,
        SafetyFlag.Chemicals,
        SafetyFlag.Flammables,
        SafetyFlag.HazmatShipping,
        SafetyFlag.Fleet,
        SafetyFlag.EnvironmentalPermits,
        SafetyFlag.ExplosionRisk,
        SafetyFlag.ConfinedSpaces,
        SafetyFlag.LOTO,
        SafetyFlag.Dust,
    };

    /// <summary>
    /// Decomposes a combined <see cref="SafetyFlag"/> bitmask into its constituent single-bit
    /// values, in <see cref="AllFlags"/> order.
    /// </summary>
    public static IReadOnlyList<SafetyFlag> Decompose(SafetyFlag flags) =>
        AllFlags.Where(flag => flags.HasFlag(flag)).ToArray();
}
