// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Consumers
//  Layer:          Abstractions
//  File:           SopsRegistry.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic Industry -> Standard Operating Procedure
//                  template mapping, derived entirely from
//                  IndustryDefinition.Flags (SafetyFlag) via
//                  IndustryRegistry.All. Introduces no new industries,
//                  regulators, or flags -- SopTemplateCatalog.ForFlag is a
//                  total, 1:1 mapping from each existing SafetyFlag bit to a
//                  canonical SOP template. Mirrors HazardMapping.cs's shape.
//                  Additive-only evolution -- SopTemplateCatalog.ForFlag's
//                  Code/Title values must never change meaning once assigned,
//                  and must remain total over every SafetyFlagCatalog.AllFlags
//                  value.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.Consumers;

/// <summary>A Standard Operating Procedure template, identified by a stable code and title.</summary>
/// <param name="Code">Stable, machine-referenceable SOP template code.</param>
/// <param name="Title">Human-readable SOP template title.</param>
public sealed record SopTemplate(string Code, string Title);

/// <summary>Total, 1:1 mapping from each single-bit <see cref="SafetyFlag"/> to its <see cref="SopTemplate"/>.</summary>
public static class SopTemplateCatalog
{
    /// <summary>Returns the <see cref="SopTemplate"/> corresponding to a single-bit <see cref="SafetyFlag"/> value.</summary>
    public static SopTemplate ForFlag(SafetyFlag flag) => flag switch
    {
        SafetyFlag.Machinery => new SopTemplate("SOP-MACHINERY", "Machine Guarding and Safe Operation"),
        SafetyFlag.Chemicals => new SopTemplate("SOP-CHEMICALS", "Chemical Handling and Storage"),
        SafetyFlag.Flammables => new SopTemplate("SOP-FLAMMABLES", "Flammable Materials Handling"),
        SafetyFlag.HazmatShipping => new SopTemplate("SOP-HAZMAT-SHIPPING", "Hazardous Materials Shipping"),
        SafetyFlag.Fleet => new SopTemplate("SOP-FLEET", "Vehicle Fleet Operation"),
        SafetyFlag.EnvironmentalPermits => new SopTemplate("SOP-ENV-PERMITS", "Environmental Permit Compliance"),
        SafetyFlag.ExplosionRisk => new SopTemplate("SOP-EXPLOSION-RISK", "Explosion Prevention and Response"),
        SafetyFlag.ConfinedSpaces => new SopTemplate("SOP-CONFINED-SPACES", "Confined Space Entry"),
        SafetyFlag.LOTO => new SopTemplate("SOP-LOTO", "Lockout/Tagout Energy Control"),
        SafetyFlag.Dust => new SopTemplate("SOP-DUST", "Combustible Dust Control"),
        _ => throw new ArgumentOutOfRangeException(nameof(flag), flag, "Unknown or non-single-bit SafetyFlag."),
    };
}

/// <summary>
/// Deterministic Industry -> SOP template mapping, computed from
/// <see cref="IndustryRegistry.All"/> and each industry's <see cref="IndustryDefinition.Flags"/>.
/// </summary>
public static class SopsRegistry
{
    /// <summary>Every SOP template applicable to the given industry, in <see cref="SafetyFlagCatalog.AllFlags"/> order.</summary>
    public static IReadOnlyList<SopTemplate> For(IndustryDefinition industry) =>
        SafetyFlagCatalog.Decompose(industry.Flags).Select(SopTemplateCatalog.ForFlag).ToArray();

    /// <summary>SOP templates for every industry in <see cref="IndustryRegistry.All"/>, keyed by <see cref="IndustryDefinition.Name"/>.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<SopTemplate>> ByIndustryName { get; } =
        IndustryRegistry.All.ToDictionary(
            industry => industry.Name,
            For,
            StringComparer.Ordinal);
}
