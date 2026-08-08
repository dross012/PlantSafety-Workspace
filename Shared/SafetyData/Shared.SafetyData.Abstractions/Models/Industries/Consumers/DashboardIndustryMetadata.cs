// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Consumers
//  Layer:          Abstractions
//  File:           DashboardIndustryMetadata.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic Industry -> Dashboard summary metadata,
//                  derived entirely from IndustryDefinition (Name,
//                  TopLevelCategory, Flags, Regulators) via
//                  IndustryRegistry.All. Introduces no new industries,
//                  regulators, or flags. RiskTier is computed purely from the
//                  count of SafetyFlagCatalog.Decompose(industry.Flags) --
//                  across the 40 industries in IndustryRegistry.All this
//                  count ranges from 4 (Medical Devices) to 9 (Chemical
//                  Manufacturing, Petrochemical & Refining); the thresholds
//                  below (<=5 / <=7 / >7) split that observed range into
//                  three roughly-even tiers. Additive-only evolution -- the
//                  RiskTier thresholds must not change without re-validating
//                  against the full IndustryRegistry.All flag-count range.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.Consumers;

/// <summary>Relative hazard-flag density tier for an industry's dashboard summary card.</summary>
public enum RiskTier
{
    /// <summary>4-5 hazard flags set (the lower half of the observed 4-9 range).</summary>
    Standard,

    /// <summary>6-7 hazard flags set.</summary>
    Elevated,

    /// <summary>8-9 hazard flags set (the upper end of the observed range).</summary>
    Critical,
}

/// <summary>
/// Dashboard-facing summary metadata for a single industry.
/// </summary>
/// <param name="Name">The industry's canonical name (<see cref="IndustryDefinition.Name"/>).</param>
/// <param name="TopLevelCategory">The industry's top-level taxonomy category (<see cref="IndustryDefinition.TopLevelCategory"/>).</param>
/// <param name="HazardFlagCount">Count of single-bit <see cref="SafetyFlag"/> values set on this industry.</param>
/// <param name="RegulatorCount">Count of <see cref="Regulator"/> values applicable to this industry.</param>
/// <param name="RiskTier">The hazard-flag-density tier derived from <paramref name="HazardFlagCount"/>.</param>
public sealed record DashboardIndustryMetadata(
    string Name,
    string TopLevelCategory,
    int HazardFlagCount,
    int RegulatorCount,
    RiskTier RiskTier);

/// <summary>
/// Deterministic Industry -> Dashboard metadata mapping, computed from
/// <see cref="IndustryRegistry.All"/>.
/// </summary>
public static class DashboardMetadataRegistry
{
    /// <summary>Derives the <see cref="RiskTier"/> for a given <see cref="SafetyFlag"/> bitmask.</summary>
    public static RiskTier RiskTierFor(SafetyFlag flags) => SafetyFlagCatalog.Decompose(flags).Count switch
    {
        <= 5 => RiskTier.Standard,
        <= 7 => RiskTier.Elevated,
        _ => RiskTier.Critical,
    };

    /// <summary>Builds the <see cref="DashboardIndustryMetadata"/> for a single industry.</summary>
    public static DashboardIndustryMetadata For(IndustryDefinition industry) => new(
        Name: industry.Name,
        TopLevelCategory: industry.TopLevelCategory,
        HazardFlagCount: SafetyFlagCatalog.Decompose(industry.Flags).Count,
        RegulatorCount: industry.Regulators.Count,
        RiskTier: RiskTierFor(industry.Flags));

    /// <summary>Dashboard metadata for every industry in <see cref="IndustryRegistry.All"/>, in taxonomy order.</summary>
    public static IReadOnlyList<DashboardIndustryMetadata> All { get; } =
        IndustryRegistry.All.Select(For).ToArray();

    /// <summary>Dashboard metadata for every industry, keyed by <see cref="IndustryDefinition.Name"/>.</summary>
    public static IReadOnlyDictionary<string, DashboardIndustryMetadata> ByIndustryName { get; } =
        All.ToDictionary(metadata => metadata.Name, StringComparer.Ordinal);
}
