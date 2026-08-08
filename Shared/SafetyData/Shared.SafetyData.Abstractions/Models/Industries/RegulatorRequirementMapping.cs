// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries
//  Layer:          Abstractions
//  File:           RegulatorRequirementMapping.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic Industry -> Regulatory requirement mapping,
//                  computed from IndustryRegistry.All and each industry's
//                  IndustryDefinition.Regulators via
//                  RegulatorRequirementCatalog.ForRegulator. No hand-authored
//                  per-industry content -- an industry's requirements are
//                  exactly the union of RegulatorRequirementCatalog entries
//                  for every regulator in its Regulators list, in that list's
//                  order.
//
//                  IndustryDefinition.Flags is deliberately not consulted
//                  here: RegulatorRequirement is scoped strictly to
//                  Regulator (see RegulatorRequirement.cs), and flag-driven
//                  obligations are already covered by the dedicated,
//                  flag-keyed consumers in Models/Industries/Consumers/
//                  (HazardMapping, RequiredSafetyModules, SopsRegistry,
//                  AuditChecklistRegistry, EventRoutingRules). Mixing a
//                  flag-derived filter into this Regulator-scoped mapping
//                  would either duplicate those consumers or silently drop
//                  real regulatory requirements for flags that happen not to
//                  correlate with a given regulator -- both would be
//                  non-deterministic drift, not a real derivation.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries;

/// <summary>
/// Deterministic Industry -> Regulatory requirement mapping, computed from
/// <see cref="IndustryRegistry.All"/> and each industry's
/// <see cref="IndustryDefinition.Regulators"/>.
/// </summary>
public static class RegulatorRequirementMapping
{
    /// <summary>
    /// Every regulatory requirement applicable to the given industry: the union of
    /// <see cref="RegulatorRequirementCatalog.ForRegulator"/> for each regulator in
    /// <see cref="IndustryDefinition.Regulators"/>, in that list's order.
    /// </summary>
    public static IReadOnlyList<RegulatorRequirement> For(IndustryDefinition industry) =>
        industry.Regulators.SelectMany(RegulatorRequirementCatalog.ForRegulator).ToArray();

    /// <summary>Regulatory requirements for every industry in <see cref="IndustryRegistry.All"/>, keyed by <see cref="IndustryDefinition.Name"/>.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<RegulatorRequirement>> All { get; } =
        IndustryRegistry.All.ToDictionary(
            industry => industry.Name,
            For,
            StringComparer.Ordinal);

    /// <summary>
    /// Every regulatory requirement applicable to the industry with the given exact,
    /// canonical name, or an empty list if no such industry exists in
    /// <see cref="IndustryRegistry.All"/>.
    /// </summary>
    public static IReadOnlyList<RegulatorRequirement> ByIndustryName(string name) =>
        All.TryGetValue(name, out IReadOnlyList<RegulatorRequirement>? requirements)
            ? requirements
            : Array.Empty<RegulatorRequirement>();
}
