// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries
//  Layer:          Abstractions
//  File:           ProgramDefinition.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Immutable schema for a single Safety Program -- the
//                  operationalized, facility-facing form of one
//                  RegulatorRequirementCatalog entry (see ProgramCatalog.cs,
//                  which builds every ProgramDefinition instance 1:1 from
//                  RegulatorRequirementCatalog.All; nothing here is
//                  hand-authored). PrimaryFlag is nullable because not every
//                  regulatory requirement corresponds to one of the 10
//                  existing SafetyFlag values (e.g. FDA's GMP/Sanitation/
//                  Quality Systems requirements are quality/food-safety
//                  programs with no hazard-flag correlate in this system) --
//                  leaving it null in that case is the honest answer, not a
//                  gap to be forced shut with an invented flag. Mirrors
//                  RegulatorRequirement.cs's shape/placement. Additive-only
//                  evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries;

/// <summary>
/// A single Safety Program a facility must implement: the operational form of one
/// <see cref="RegulatorRequirement"/>.
/// </summary>
/// <param name="Code">Stable, machine-referenceable program code (e.g. "PROG-OSHA-01").</param>
/// <param name="Name">The program's canonical name, copied from its source <see cref="RegulatorRequirement.Name"/>.</param>
/// <param name="Description">The program's description, copied from its source <see cref="RegulatorRequirement.Description"/>.</param>
/// <param name="PrimaryRegulator">The regulator this program's source requirement is drawn from.</param>
/// <param name="Category">The <see cref="RegulatorCategory"/> <paramref name="PrimaryRegulator"/> maps onto.</param>
/// <param name="PrimaryFlag">
/// The single <see cref="SafetyFlag"/> this program most directly operationalizes, if one exists
/// (see <see cref="ProgramCatalog"/> for the derivation rule); <see langword="null"/> when the
/// program's domain (e.g. FDA quality/sanitation programs) has no hazard-flag correlate.
/// </param>
public sealed record ProgramDefinition(
    string Code,
    string Name,
    string Description,
    Regulator PrimaryRegulator,
    RegulatorCategory Category,
    SafetyFlag? PrimaryFlag);
