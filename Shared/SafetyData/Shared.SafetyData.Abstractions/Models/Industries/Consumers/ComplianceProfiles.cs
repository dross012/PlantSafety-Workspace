// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Consumers
//  Layer:          Abstractions
//  File:           ComplianceProfiles.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Composite Industry -> Compliance profile mapping, computed
//                  from IndustryRegistry.All plus every other Industry
//                  subsystem consumer in this folder (HazardMapping,
//                  RequiredSafetyModules, PermitRequirements, SopsRegistry,
//                  AuditChecklistRegistry, EventRoutingRules). Introduces no
//                  new industries, regulators, flags, hazards, modules,
//                  permits, SOPs, checklist items, or topics -- ComplianceProfile
//                  is purely an aggregation of the other consumers' already-
//                  deterministic per-industry output. Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models.Industries.Consumers;

/// <summary>
/// A single industry's full compliance profile: its taxonomy identity plus every
/// derived consumer artifact (hazards, required modules, permits, SOP templates,
/// audit checklist, and event routing topics).
/// </summary>
/// <param name="IndustryName">The industry's canonical name (<see cref="IndustryDefinition.Name"/>).</param>
/// <param name="TopLevelCategory">The industry's top-level taxonomy category.</param>
/// <param name="SubIndustries">The industry's named sub-industries (may be empty).</param>
/// <param name="Regulators">The regulators applicable to this industry.</param>
/// <param name="Flags">The combined safety/compliance flags applicable to this industry.</param>
/// <param name="Hazards">Hazard categories derived from <paramref name="Flags"/> (<see cref="HazardMapping"/>).</param>
/// <param name="RequiredModules">Required PlantSafety modules derived from <paramref name="Flags"/> (<see cref="RequiredSafetyModules"/>).</param>
/// <param name="RequiredPermits">Required permit types derived from <paramref name="Regulators"/> (<see cref="PermitRequirements"/>).</param>
/// <param name="SopTemplates">SOP templates derived from <paramref name="Flags"/> (<see cref="SopsRegistry"/>).</param>
/// <param name="AuditChecklist">Audit checklist items derived from <paramref name="Flags"/> and <paramref name="Regulators"/> (<see cref="AuditChecklistRegistry"/>).</param>
/// <param name="EventRoutingTopics">Event-bus topics this industry's events must be routed to (<see cref="EventRoutingRules"/>).</param>
public sealed record ComplianceProfile(
    string IndustryName,
    string TopLevelCategory,
    IReadOnlyList<string> SubIndustries,
    IReadOnlyList<Regulator> Regulators,
    SafetyFlag Flags,
    IReadOnlyList<HazardCategory> Hazards,
    IReadOnlyList<SafetyModule> RequiredModules,
    IReadOnlyList<PermitType> RequiredPermits,
    IReadOnlyList<SopTemplate> SopTemplates,
    IReadOnlyList<AuditChecklistItem> AuditChecklist,
    IReadOnlyList<string> EventRoutingTopics);

/// <summary>
/// Deterministic Industry -> Compliance profile mapping, computed from
/// <see cref="IndustryRegistry.All"/> and every other Industry subsystem consumer.
/// </summary>
public static class ComplianceProfiles
{
    /// <summary>Builds the <see cref="ComplianceProfile"/> for a single industry.</summary>
    public static ComplianceProfile For(IndustryDefinition industry) => new(
        IndustryName: industry.Name,
        TopLevelCategory: industry.TopLevelCategory,
        SubIndustries: industry.SubIndustries,
        Regulators: industry.Regulators,
        Flags: industry.Flags,
        Hazards: HazardMapping.For(industry),
        RequiredModules: RequiredSafetyModules.For(industry),
        RequiredPermits: PermitRequirements.For(industry),
        SopTemplates: SopsRegistry.For(industry),
        AuditChecklist: AuditChecklistRegistry.For(industry),
        EventRoutingTopics: EventRoutingRules.TopicsFor(industry));

    /// <summary>Compliance profiles for every industry in <see cref="IndustryRegistry.All"/>, in taxonomy order.</summary>
    public static IReadOnlyList<ComplianceProfile> All { get; } =
        IndustryRegistry.All.Select(For).ToArray();

    /// <summary>Compliance profiles for every industry, keyed by <see cref="IndustryDefinition.Name"/>.</summary>
    public static IReadOnlyDictionary<string, ComplianceProfile> ByIndustryName { get; } =
        All.ToDictionary(profile => profile.IndustryName, StringComparer.Ordinal);

    /// <summary>Looks up a compliance profile by its exact industry name.</summary>
    public static ComplianceProfile? FindByIndustryName(string industryName) =>
        ByIndustryName.TryGetValue(industryName, out ComplianceProfile? profile) ? profile : null;
}
