// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Consumers
//  Layer:          Abstractions
//  File:           RequiredSafetyModules.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic Industry -> Required PlantSafety platform
//                  module mapping, derived entirely from
//                  IndustryDefinition.Flags (SafetyFlag) via
//                  IndustryRegistry.All. Introduces no new industries,
//                  regulators, or flags -- SafetyModule is a 1:1 restatement
//                  of the existing SafetyFlag bits as platform modules a
//                  tenant in that industry must have enabled (see
//                  SafetyModuleCatalog.ForFlag). Mirrors HazardMapping.cs's
//                  shape. Additive-only evolution -- SafetyModule members
//                  must never be renamed or removed, and ForFlag must remain
//                  a total, 1:1 mapping over every SafetyFlagCatalog.AllFlags
//                  value.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.Consumers;

/// <summary>
/// A PlantSafety platform module required for tenants in a given industry,
/// derived 1:1 from a single <see cref="SafetyFlag"/> bit.
/// </summary>
public enum SafetyModule
{
    /// <summary>Machine guarding &amp; safe-operation tracking (from <see cref="SafetyFlag.Machinery"/>).</summary>
    MachineGuardingModule,

    /// <summary>Chemical inventory &amp; SDS management (from <see cref="SafetyFlag.Chemicals"/>).</summary>
    ChemicalInventoryModule,

    /// <summary>Hot work / flammable-materials permit tracking (from <see cref="SafetyFlag.Flammables"/>).</summary>
    HotWorkPermitModule,

    /// <summary>Hazardous-materials shipping documentation &amp; tracking (from <see cref="SafetyFlag.HazmatShipping"/>).</summary>
    HazmatShippingModule,

    /// <summary>Vehicle fleet safety tracking (from <see cref="SafetyFlag.Fleet"/>).</summary>
    FleetSafetyModule,

    /// <summary>Environmental permit tracking &amp; renewal (from <see cref="SafetyFlag.EnvironmentalPermits"/>).</summary>
    EnvironmentalPermitTrackingModule,

    /// <summary>Explosion prevention &amp; process safety management (from <see cref="SafetyFlag.ExplosionRisk"/>).</summary>
    ExplosionPreventionModule,

    /// <summary>Confined space entry permit tracking (from <see cref="SafetyFlag.ConfinedSpaces"/>).</summary>
    ConfinedSpaceEntryModule,

    /// <summary>Lockout/Tagout energy-control tracking (from <see cref="SafetyFlag.LOTO"/>).</summary>
    LockoutTagoutModule,

    /// <summary>Combustible dust control &amp; housekeeping tracking (from <see cref="SafetyFlag.Dust"/>).</summary>
    DustControlModule,
}

/// <summary>Total, 1:1 mapping from each single-bit <see cref="SafetyFlag"/> to its required <see cref="SafetyModule"/>.</summary>
public static class SafetyModuleCatalog
{
    /// <summary>Returns the <see cref="SafetyModule"/> corresponding to a single-bit <see cref="SafetyFlag"/> value.</summary>
    public static SafetyModule ForFlag(SafetyFlag flag) => flag switch
    {
        SafetyFlag.Machinery => SafetyModule.MachineGuardingModule,
        SafetyFlag.Chemicals => SafetyModule.ChemicalInventoryModule,
        SafetyFlag.Flammables => SafetyModule.HotWorkPermitModule,
        SafetyFlag.HazmatShipping => SafetyModule.HazmatShippingModule,
        SafetyFlag.Fleet => SafetyModule.FleetSafetyModule,
        SafetyFlag.EnvironmentalPermits => SafetyModule.EnvironmentalPermitTrackingModule,
        SafetyFlag.ExplosionRisk => SafetyModule.ExplosionPreventionModule,
        SafetyFlag.ConfinedSpaces => SafetyModule.ConfinedSpaceEntryModule,
        SafetyFlag.LOTO => SafetyModule.LockoutTagoutModule,
        SafetyFlag.Dust => SafetyModule.DustControlModule,
        _ => throw new ArgumentOutOfRangeException(nameof(flag), flag, "Unknown or non-single-bit SafetyFlag."),
    };
}

/// <summary>
/// Deterministic Industry -> Required PlantSafety module mapping, computed from
/// <see cref="IndustryRegistry.All"/> and each industry's <see cref="IndustryDefinition.Flags"/>.
/// </summary>
public static class RequiredSafetyModules
{
    /// <summary>Every module required for the given industry, in <see cref="SafetyFlagCatalog.AllFlags"/> order.</summary>
    public static IReadOnlyList<SafetyModule> For(IndustryDefinition industry) =>
        SafetyFlagCatalog.Decompose(industry.Flags).Select(SafetyModuleCatalog.ForFlag).ToArray();

    /// <summary>Required modules for every industry in <see cref="IndustryRegistry.All"/>, keyed by <see cref="IndustryDefinition.Name"/>.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<SafetyModule>> ByIndustryName { get; } =
        IndustryRegistry.All.ToDictionary(
            industry => industry.Name,
            For,
            StringComparer.Ordinal);
}
