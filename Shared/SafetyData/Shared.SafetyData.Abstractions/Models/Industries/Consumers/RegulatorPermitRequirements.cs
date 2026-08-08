// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Consumers
//  Layer:          Abstractions
//  File:           RegulatorPermitRequirements.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic Regulator -> Permit requirement mapping.
//                  Introduces no new permit category type -- reuses the
//                  existing PermitType enum and PermitTypeCatalog.ForRegulator
//                  exactly as defined (see PermitRequirements.cs), which are
//                  already a total, 1:1 mapping from every Regulator value to
//                  its canonical permit/filing type. This file exists to give
//                  the Regulatory Subsystem its own Regulator-first entry
//                  point (ForRegulator/ByRegulator), matching the shape of
//                  RegulatorSafetyModuleMapping.cs and
//                  RegulatorSopRequirements.cs, rather than duplicating
//                  PermitTypeCatalog's derivation logic. Additive-only
//                  evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries;

namespace Shared.SafetyData.Abstractions.Models.Industries.Consumers;

/// <summary>
/// Deterministic Regulator -> Permit requirement mapping, computed from
/// <see cref="PermitTypeCatalog.ForRegulator"/>.
/// </summary>
public static class RegulatorPermitRequirements
{
    /// <summary>The single canonical <see cref="PermitType"/> required by the given regulator.</summary>
    public static IReadOnlyList<PermitType> ForRegulator(Regulator regulator) =>
        new[] { PermitTypeCatalog.ForRegulator(regulator) };

    /// <summary>Permit requirements for every <see cref="Regulator"/> value, in enum declaration order.</summary>
    public static IReadOnlyDictionary<Regulator, IReadOnlyList<PermitType>> ByRegulator { get; } =
        Enum.GetValues<Regulator>().ToDictionary(regulator => regulator, ForRegulator);

    /// <summary>
    /// Every distinct permit type required across all of the given industry's regulators, in
    /// <see cref="IndustryDefinition.Regulators"/> order. Equivalent to <see cref="PermitRequirements.For"/>.
    /// </summary>
    public static IReadOnlyList<PermitType> For(IndustryDefinition industry) =>
        industry.Regulators.SelectMany(ForRegulator).Distinct().ToArray();
}
