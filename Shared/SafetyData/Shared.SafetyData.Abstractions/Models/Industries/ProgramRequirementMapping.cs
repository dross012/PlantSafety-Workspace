// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries
//  Layer:          Abstractions
//  File:           ProgramRequirementMapping.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic Industry -> Applicable Safety Programs
//                  mapping, computed from IndustryRegistry.All plus:
//                    - RegulatorRequirementMapping: a program applies via its
//                      PrimaryRegulator if the industry's own
//                      RegulatorRequirementMapping.For(industry) output
//                      contains a requirement with the same Name as the
//                      program's source requirement (equivalent to, but more
//                      traceable than, a raw industry.Regulators.Contains
//                      check, since it goes through the same dependency the
//                      Regulatory Subsystem itself uses).
//                    - HazardMapping and RequiredSafetyModules: a program
//                      also applies via its PrimaryFlag (when not null) if
//                      that flag's derived hazard category appears in
//                      HazardMapping.For(industry) AND that flag's derived
//                      module appears in RequiredSafetyModules.For(industry)
//                      -- both must agree, since both are themselves
//                      independently derived from the same
//                      SafetyFlagCatalog.Decompose(industry.Flags), so
//                      requiring both is a consistency check, not two
//                      separate criteria.
//                  A program applies to an industry if either path holds. No
//                  hand-authored per-industry content. Additive-only
//                  evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries;

using Shared.SafetyData.Abstractions.Models.Industries.Consumers;

/// <summary>
/// Deterministic Industry -> Applicable Safety Programs mapping, computed from
/// <see cref="IndustryRegistry.All"/>, <see cref="RegulatorRequirementMapping"/>,
/// <see cref="HazardMapping"/>, and <see cref="RequiredSafetyModules"/>.
/// </summary>
public static class ProgramRequirementMapping
{
    /// <summary>
    /// Returns whether <paramref name="program"/> applies to <paramref name="industry"/>, via
    /// its regulator or its primary hazard flag (see this file's header for the two paths).
    /// </summary>
    public static bool AppliesTo(ProgramDefinition program, IndustryDefinition industry)
    {
        bool regulatorApplies = RegulatorRequirementMapping.For(industry)
            .Any(requirement => requirement.Regulator == program.PrimaryRegulator && requirement.Name == program.Name);

        bool flagApplies = program.PrimaryFlag is SafetyFlag flag
            && HazardMapping.For(industry).Contains(HazardCategoryCatalog.ForFlag(flag))
            && RequiredSafetyModules.For(industry).Contains(SafetyModuleCatalog.ForFlag(flag));

        return regulatorApplies || flagApplies;
    }

    /// <summary>Every Safety Program applicable to the given industry, in <see cref="ProgramCatalog.All"/> order.</summary>
    public static IReadOnlyList<ProgramDefinition> For(IndustryDefinition industry) =>
        ProgramCatalog.All.Where(program => AppliesTo(program, industry)).ToArray();

    /// <summary>Applicable programs for every industry in <see cref="IndustryRegistry.All"/>, keyed by <see cref="IndustryDefinition.Name"/>.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<ProgramDefinition>> All { get; } =
        IndustryRegistry.All.ToDictionary(
            industry => industry.Name,
            For,
            StringComparer.Ordinal);

    /// <summary>
    /// Every Safety Program applicable to the industry with the given exact, canonical name, or
    /// an empty list if no such industry exists in <see cref="IndustryRegistry.All"/>.
    /// </summary>
    public static IReadOnlyList<ProgramDefinition> ByIndustryName(string name) =>
        All.TryGetValue(name, out IReadOnlyList<ProgramDefinition>? programs) ? programs : Array.Empty<ProgramDefinition>();
}
