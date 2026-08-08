// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Consumers
//  Layer:          Abstractions
//  File:           HazardMapping.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic Industry -> Hazard category mapping, derived
//                  entirely from IndustryDefinition.Flags (SafetyFlag) via
//                  IndustryRegistry.All. Introduces no new industries,
//                  regulators, or flags -- HazardCategory is a 1:1 restatement
//                  of the existing SafetyFlag bits as audit/reporting-facing
//                  hazard categories (see HazardCategoryCatalog.ForFlag).
//                  Additive-only evolution -- HazardCategory members must
//                  never be renamed or removed, and ForFlag must remain a
//                  total, 1:1 mapping over every SafetyFlagCatalog.AllFlags
//                  value.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.Consumers;

/// <summary>
/// A hazard category surfaced to auditors/reporting, derived 1:1 from a single
/// <see cref="SafetyFlag"/> bit.
/// </summary>
public enum HazardCategory
{
    /// <summary>Rotating/moving industrial machinery hazard (from <see cref="SafetyFlag.Machinery"/>).</summary>
    MachineryHazard,

    /// <summary>Hazardous chemical handling/exposure hazard (from <see cref="SafetyFlag.Chemicals"/>).</summary>
    ChemicalExposureHazard,

    /// <summary>Fire hazard from flammable materials/atmospheres (from <see cref="SafetyFlag.Flammables"/>).</summary>
    FireHazard,

    /// <summary>Hazardous-materials transport hazard (from <see cref="SafetyFlag.HazmatShipping"/>).</summary>
    HazmatTransportHazard,

    /// <summary>Vehicle fleet operating hazard (from <see cref="SafetyFlag.Fleet"/>).</summary>
    VehicleFleetHazard,

    /// <summary>Environmental release/permit-compliance hazard (from <see cref="SafetyFlag.EnvironmentalPermits"/>).</summary>
    EnvironmentalReleaseHazard,

    /// <summary>Explosion hazard (from <see cref="SafetyFlag.ExplosionRisk"/>).</summary>
    ExplosionHazard,

    /// <summary>Confined space entry hazard (from <see cref="SafetyFlag.ConfinedSpaces"/>).</summary>
    ConfinedSpaceHazard,

    /// <summary>Stored/hazardous energy release hazard (from <see cref="SafetyFlag.LOTO"/>).</summary>
    StoredEnergyHazard,

    /// <summary>Combustible/respirable dust hazard (from <see cref="SafetyFlag.Dust"/>).</summary>
    CombustibleDustHazard,
}

/// <summary>Total, 1:1 mapping from each single-bit <see cref="SafetyFlag"/> to its <see cref="HazardCategory"/>.</summary>
public static class HazardCategoryCatalog
{
    /// <summary>Returns the <see cref="HazardCategory"/> corresponding to a single-bit <see cref="SafetyFlag"/> value.</summary>
    public static HazardCategory ForFlag(SafetyFlag flag) => flag switch
    {
        SafetyFlag.Machinery => HazardCategory.MachineryHazard,
        SafetyFlag.Chemicals => HazardCategory.ChemicalExposureHazard,
        SafetyFlag.Flammables => HazardCategory.FireHazard,
        SafetyFlag.HazmatShipping => HazardCategory.HazmatTransportHazard,
        SafetyFlag.Fleet => HazardCategory.VehicleFleetHazard,
        SafetyFlag.EnvironmentalPermits => HazardCategory.EnvironmentalReleaseHazard,
        SafetyFlag.ExplosionRisk => HazardCategory.ExplosionHazard,
        SafetyFlag.ConfinedSpaces => HazardCategory.ConfinedSpaceHazard,
        SafetyFlag.LOTO => HazardCategory.StoredEnergyHazard,
        SafetyFlag.Dust => HazardCategory.CombustibleDustHazard,
        _ => throw new ArgumentOutOfRangeException(nameof(flag), flag, "Unknown or non-single-bit SafetyFlag."),
    };
}

/// <summary>
/// Deterministic Industry -> Hazard category mapping, computed from
/// <see cref="IndustryRegistry.All"/> and each industry's <see cref="IndustryDefinition.Flags"/>.
/// </summary>
public static class HazardMapping
{
    /// <summary>Every hazard category applicable to the given industry, in <see cref="SafetyFlagCatalog.AllFlags"/> order.</summary>
    public static IReadOnlyList<HazardCategory> For(IndustryDefinition industry) =>
        SafetyFlagCatalog.Decompose(industry.Flags).Select(HazardCategoryCatalog.ForFlag).ToArray();

    /// <summary>Hazard categories for every industry in <see cref="IndustryRegistry.All"/>, keyed by <see cref="IndustryDefinition.Name"/>.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<HazardCategory>> ByIndustryName { get; } =
        IndustryRegistry.All.ToDictionary(
            industry => industry.Name,
            For,
            StringComparer.Ordinal);
}
