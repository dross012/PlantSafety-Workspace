// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Consumers
//  Layer:          Abstractions
//  File:           RegulatorSafetyModuleMapping.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic Regulator -> Required PlantSafety module
//                  mapping. Introduces no new modules -- reuses the existing
//                  SafetyModule enum and SafetyModuleCatalog.ForFlag (see
//                  RequiredSafetyModules.cs) exactly as defined.
//
//                  Derivation: a Regulator has no direct SafetyFlag of its
//                  own, so the required-module set for a given regulator is
//                  computed from real, existing data rather than invented --
//                  it is the union of SafetyFlagCatalog.Decompose(industry.Flags)
//                  across every industry in IndustryRegistry.All whose
//                  Regulators list contains that regulator, mapped to
//                  SafetyModule via SafetyModuleCatalog.ForFlag. In other
//                  words: "which safety modules does every real industry that
//                  answers to this regulator actually need, per the taxonomy
//                  already on file." No hand-authored per-regulator module
//                  list. Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries;

namespace Shared.SafetyData.Abstractions.Models.Industries.Consumers;

/// <summary>
/// Deterministic Regulator -> Required <see cref="SafetyModule"/> mapping, computed from
/// <see cref="IndustryRegistry.All"/>: for each regulator, the union of
/// <see cref="SafetyModuleCatalog.ForFlag"/> over every flag set on any industry that carries
/// that regulator.
/// </summary>
public static class RegulatorSafetyModuleMapping
{
    /// <summary>
    /// Every <see cref="SafetyModule"/> required by industries that answer to the given
    /// regulator, in <see cref="SafetyFlagCatalog.AllFlags"/> order.
    /// </summary>
    public static IReadOnlyList<SafetyModule> ForRegulator(Regulator regulator)
    {
        SafetyFlag combinedFlags = IndustryRegistry.All
            .Where(industry => industry.Regulators.Contains(regulator))
            .Aggregate(SafetyFlag.None, (accumulated, industry) => accumulated | industry.Flags);

        return SafetyFlagCatalog.Decompose(combinedFlags).Select(SafetyModuleCatalog.ForFlag).ToArray();
    }

    /// <summary>Required safety modules for every <see cref="Regulator"/> value, in enum declaration order.</summary>
    public static IReadOnlyDictionary<Regulator, IReadOnlyList<SafetyModule>> ByRegulator { get; } =
        Enum.GetValues<Regulator>().ToDictionary(regulator => regulator, ForRegulator);

    /// <summary>
    /// Every distinct <see cref="SafetyModule"/> required across all of the given industry's
    /// regulators, in <see cref="IndustryDefinition.Regulators"/> order.
    /// </summary>
    public static IReadOnlyList<SafetyModule> For(IndustryDefinition industry) =>
        industry.Regulators.SelectMany(ForRegulator).Distinct().ToArray();
}
