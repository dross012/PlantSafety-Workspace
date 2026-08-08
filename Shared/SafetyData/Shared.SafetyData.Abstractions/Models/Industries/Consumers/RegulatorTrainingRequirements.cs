// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Consumers
//  Layer:          Abstractions
//  File:           RegulatorTrainingRequirements.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic Regulator -> Training requirement mapping.
//                  TrainingRequirement is a new record (no existing training
//                  model to reuse), but its instances are never hand-authored:
//                  each regulator's training requirements are computed
//                  directly from that regulator's own entries in
//                  RegulatorRequirementCatalog.ForRegulator -- the same
//                  already-cataloged regulatory domain used throughout this
//                  subsystem -- one TrainingRequirement per requirement, never
//                  a separately invented training topic list. Mirrors
//                  RegulatorSopRequirements.cs's derivation shape exactly
//                  (Code built from regulator + 1-based catalog position,
//                  Title from the requirement's own Name). Additive-only
//                  evolution -- reordering or removing entries from
//                  RegulatorRequirementCatalog would change the codes
//                  generated here, so that catalog's additive-only invariant
//                  is what keeps these codes stable.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries;

namespace Shared.SafetyData.Abstractions.Models.Industries.Consumers;

/// <summary>A training requirement, identified by a stable code and title.</summary>
/// <param name="Code">Stable, machine-referenceable training requirement code.</param>
/// <param name="Title">Human-readable training topic title.</param>
public sealed record TrainingRequirement(string Code, string Title);

/// <summary>
/// Deterministic Regulator -> Training requirement mapping, computed from
/// <see cref="RegulatorRequirementCatalog.ForRegulator"/>.
/// </summary>
public static class RegulatorTrainingRequirements
{
    /// <summary>
    /// One <see cref="TrainingRequirement"/> per <see cref="RegulatorRequirementCatalog.ForRegulator"/> entry
    /// for the given regulator, in that catalog's order.
    /// </summary>
    public static IReadOnlyList<TrainingRequirement> ForRegulator(Regulator regulator) =>
        RegulatorRequirementCatalog.ForRegulator(regulator)
            .Select((requirement, index) => new TrainingRequirement($"TRN-{regulator}-{index + 1:D2}", $"{requirement.Name} Training"))
            .ToArray();

    /// <summary>Training requirements for every <see cref="Regulator"/> value, in enum declaration order.</summary>
    public static IReadOnlyDictionary<Regulator, IReadOnlyList<TrainingRequirement>> ByRegulator { get; } =
        Enum.GetValues<Regulator>().ToDictionary(regulator => regulator, ForRegulator);

    /// <summary>
    /// Every distinct training requirement across all of the given industry's regulators, in
    /// <see cref="IndustryDefinition.Regulators"/> order.
    /// </summary>
    public static IReadOnlyList<TrainingRequirement> For(IndustryDefinition industry) =>
        industry.Regulators.SelectMany(ForRegulator).Distinct().ToArray();
}
