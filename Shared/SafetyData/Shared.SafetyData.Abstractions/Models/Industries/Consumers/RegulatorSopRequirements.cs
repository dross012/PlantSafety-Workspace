// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Consumers
//  Layer:          Abstractions
//  File:           RegulatorSopRequirements.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic Regulator -> SOP requirement mapping.
//                  Introduces no new SOP category type -- reuses the existing
//                  SopTemplate record exactly as defined (see SopsRegistry.cs).
//
//                  Derivation: each regulator's SOP requirements are computed
//                  directly from that regulator's own entries in
//                  RegulatorRequirementCatalog.ForRegulator -- the same,
//                  already-cataloged regulatory domain used everywhere else in
//                  this subsystem -- one SopTemplate per requirement, never a
//                  separately hand-authored SOP topic list. Code is built
//                  deterministically from the regulator's enum name and the
//                  requirement's 1-based position within
//                  RegulatorRequirementCatalog.ForRegulator's (fixed,
//                  additive-only) order, e.g. "SOP-OSHA-01"; Title is the
//                  requirement's own Name suffixed with " SOP".
//                  Additive-only evolution -- reordering or removing entries
//                  from RegulatorRequirementCatalog would change the codes
//                  generated here, so that catalog's additive-only invariant
//                  is what keeps these codes stable.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries;

namespace Shared.SafetyData.Abstractions.Models.Industries.Consumers;

/// <summary>
/// Deterministic Regulator -> SOP requirement mapping, computed from
/// <see cref="RegulatorRequirementCatalog.ForRegulator"/>.
/// </summary>
public static class RegulatorSopRequirements
{
    /// <summary>
    /// One <see cref="SopTemplate"/> per <see cref="RegulatorRequirementCatalog.ForRegulator"/> entry
    /// for the given regulator, in that catalog's order.
    /// </summary>
    public static IReadOnlyList<SopTemplate> ForRegulator(Regulator regulator) =>
        RegulatorRequirementCatalog.ForRegulator(regulator)
            .Select((requirement, index) => new SopTemplate($"SOP-{regulator}-{index + 1:D2}", $"{requirement.Name} SOP"))
            .ToArray();

    /// <summary>SOP requirements for every <see cref="Regulator"/> value, in enum declaration order.</summary>
    public static IReadOnlyDictionary<Regulator, IReadOnlyList<SopTemplate>> ByRegulator { get; } =
        Enum.GetValues<Regulator>().ToDictionary(regulator => regulator, ForRegulator);

    /// <summary>
    /// Every distinct SOP requirement across all of the given industry's regulators, in
    /// <see cref="IndustryDefinition.Regulators"/> order.
    /// </summary>
    public static IReadOnlyList<SopTemplate> For(IndustryDefinition industry) =>
        industry.Regulators.SelectMany(ForRegulator).Distinct().ToArray();
}
