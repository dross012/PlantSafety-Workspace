// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Consumers
//  Layer:          Abstractions
//  File:           AuditChecklistRegistry.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Deterministic Industry -> Audit checklist mapping, derived
//                  entirely from IndustryDefinition.Flags (SafetyFlag) and
//                  IndustryDefinition.Regulators via IndustryRegistry.All.
//                  Introduces no new industries, regulators, or flags -- each
//                  checklist item is a total, 1:1 restatement of an existing
//                  SafetyFlag bit (hazard-control check) or Regulator value
//                  (regulatory-compliance check). An industry's checklist is
//                  the concatenation of its flag-derived items (in
//                  SafetyFlagCatalog.AllFlags order) followed by its
//                  regulator-derived items (in IndustryDefinition.Regulators
//                  order). Additive-only evolution -- AuditChecklistItem
//                  Code/Description values must never change meaning once
//                  assigned, and both catalogs must remain total.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.Consumers;

/// <summary>A single audit checklist item, identified by a stable code and description.</summary>
/// <param name="Code">Stable, machine-referenceable checklist item code.</param>
/// <param name="Description">Human-readable description of what the auditor must verify.</param>
public sealed record AuditChecklistItem(string Code, string Description);

/// <summary>
/// Total mappings from each single-bit <see cref="SafetyFlag"/> and each <see cref="Regulator"/>
/// to their respective <see cref="AuditChecklistItem"/>.
/// </summary>
public static class AuditChecklistItemCatalog
{
    /// <summary>Returns the hazard-control <see cref="AuditChecklistItem"/> for a single-bit <see cref="SafetyFlag"/> value.</summary>
    public static AuditChecklistItem ForFlag(SafetyFlag flag) => flag switch
    {
        SafetyFlag.Machinery => new AuditChecklistItem("AUDIT-MACHINERY", "Verify machine guarding is in place and functional on all covered equipment."),
        SafetyFlag.Chemicals => new AuditChecklistItem("AUDIT-CHEMICALS", "Verify chemical inventory, labeling, and Safety Data Sheets are current."),
        SafetyFlag.Flammables => new AuditChecklistItem("AUDIT-FLAMMABLES", "Verify flammable materials storage and hot work permit controls are followed."),
        SafetyFlag.HazmatShipping => new AuditChecklistItem("AUDIT-HAZMAT-SHIPPING", "Verify hazardous materials shipments are documented and labeled per PHMSA requirements."),
        SafetyFlag.Fleet => new AuditChecklistItem("AUDIT-FLEET", "Verify vehicle fleet maintenance and driver qualification records are current."),
        SafetyFlag.EnvironmentalPermits => new AuditChecklistItem("AUDIT-ENV-PERMITS", "Verify environmental permits are valid and reporting obligations are current."),
        SafetyFlag.ExplosionRisk => new AuditChecklistItem("AUDIT-EXPLOSION-RISK", "Verify explosion prevention controls and process safety reviews are current."),
        SafetyFlag.ConfinedSpaces => new AuditChecklistItem("AUDIT-CONFINED-SPACES", "Verify confined space entry permits and atmospheric testing records are current."),
        SafetyFlag.LOTO => new AuditChecklistItem("AUDIT-LOTO", "Verify lockout/tagout procedures and device inventories are current."),
        SafetyFlag.Dust => new AuditChecklistItem("AUDIT-DUST", "Verify combustible dust housekeeping and control measures are followed."),
        _ => throw new ArgumentOutOfRangeException(nameof(flag), flag, "Unknown or non-single-bit SafetyFlag."),
    };

    /// <summary>Returns the regulatory-compliance <see cref="AuditChecklistItem"/> for a <see cref="Regulator"/> value.</summary>
    public static AuditChecklistItem ForRegulator(Regulator regulator) => regulator switch
    {
        Regulator.OSHA => new AuditChecklistItem("AUDIT-REG-OSHA", "Verify OSHA workplace safety compliance program documentation is current."),
        Regulator.EPA => new AuditChecklistItem("AUDIT-REG-EPA", "Verify EPA environmental operating permit conditions are met."),
        Regulator.EPA_RCRA => new AuditChecklistItem("AUDIT-REG-EPA-RCRA", "Verify RCRA hazardous waste generator status and manifests are current."),
        Regulator.EPA_TSCA => new AuditChecklistItem("AUDIT-REG-EPA-TSCA", "Verify TSCA chemical substance reporting obligations are met."),
        Regulator.PHMSA => new AuditChecklistItem("AUDIT-REG-PHMSA", "Verify PHMSA hazmat registration and training records are current."),
        Regulator.NFPA => new AuditChecklistItem("AUDIT-REG-NFPA", "Verify NFPA fire code permit conditions and inspections are current."),
        Regulator.NFPA_Dust => new AuditChecklistItem("AUDIT-REG-NFPA-DUST", "Verify NFPA combustible dust hazard analysis is current."),
        Regulator.FMCSA => new AuditChecklistItem("AUDIT-REG-FMCSA", "Verify FMCSA motor carrier operating authority and driver files are current."),
        Regulator.DHS_CFATS => new AuditChecklistItem("AUDIT-REG-DHS-CFATS", "Verify DHS CFATS facility security plan is current."),
        Regulator.USCG => new AuditChecklistItem("AUDIT-REG-USCG", "Verify USCG facility security plan is current."),
        Regulator.FDA => new AuditChecklistItem("AUDIT-REG-FDA", "Verify FDA establishment registration and listing are current."),
        Regulator.ANSI_ISO => new AuditChecklistItem("AUDIT-REG-ANSI-ISO", "Verify ANSI/ISO consensus standard conformance certification is current."),
        Regulator.ATF => new AuditChecklistItem("AUDIT-REG-ATF", "Verify ATF explosives license and storage magazine records are current."),
        Regulator.DoD => new AuditChecklistItem("AUDIT-REG-DOD", "Verify DoD facility security clearance and controls are current."),
        Regulator.MSHA => new AuditChecklistItem("AUDIT-REG-MSHA", "Verify MSHA mine ID registration and inspection records are current."),
        _ => throw new ArgumentOutOfRangeException(nameof(regulator), regulator, "Unknown regulator."),
    };
}

/// <summary>
/// Deterministic Industry -> Audit checklist mapping, computed from
/// <see cref="IndustryRegistry.All"/> and each industry's <see cref="IndustryDefinition.Flags"/>
/// and <see cref="IndustryDefinition.Regulators"/>.
/// </summary>
public static class AuditChecklistRegistry
{
    /// <summary>
    /// The full audit checklist for the given industry: hazard-control items (in
    /// <see cref="SafetyFlagCatalog.AllFlags"/> order) followed by regulatory-compliance items
    /// (in <see cref="IndustryDefinition.Regulators"/> order).
    /// </summary>
    public static IReadOnlyList<AuditChecklistItem> For(IndustryDefinition industry) =>
        SafetyFlagCatalog.Decompose(industry.Flags).Select(AuditChecklistItemCatalog.ForFlag)
            .Concat(industry.Regulators.Select(AuditChecklistItemCatalog.ForRegulator))
            .ToArray();

    /// <summary>Audit checklists for every industry in <see cref="IndustryRegistry.All"/>, keyed by <see cref="IndustryDefinition.Name"/>.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<AuditChecklistItem>> ByIndustryName { get; } =
        IndustryRegistry.All.ToDictionary(
            industry => industry.Name,
            For,
            StringComparer.Ordinal);
}
