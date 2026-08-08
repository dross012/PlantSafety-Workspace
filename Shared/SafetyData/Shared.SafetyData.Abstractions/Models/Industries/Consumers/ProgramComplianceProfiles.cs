// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Consumers
//  Layer:          Abstractions
//  File:           ProgramComplianceProfiles.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Composite Program -> Compliance profile mapping, computed
//                  from ProgramCatalog.All plus every other Safety Programs
//                  consumer in this folder (ProgramSopRequirements,
//                  ProgramTrainingRequirements, ProgramPermitRequirements,
//                  ProgramAuditRequirements, ProgramEventRoutingRules) and
//                  ProgramRequirementMapping (for the reverse view: which
//                  industries this program applies to). Mirrors
//                  ComplianceProfiles.cs's role for the Industry subsystem,
//                  one layer up. Introduces no new programs, regulators,
//                  flags, SOPs, training topics, permits, audit items, or
//                  routing topics -- ProgramComplianceProfile is purely an
//                  aggregation of already-deterministic per-program output.
//                  Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries;

namespace Shared.SafetyData.Abstractions.Models.Industries.Consumers;

/// <summary>
/// A single Safety Program's full compliance profile: its identity plus every derived consumer
/// artifact (SOP, training, permit, audit item, event routing topic) and the industries it
/// applies to.
/// </summary>
/// <param name="ProgramCode">The program's stable code (<see cref="ProgramDefinition.Code"/>).</param>
/// <param name="ProgramName">The program's canonical name.</param>
/// <param name="PrimaryRegulator">The regulator this program's source requirement is drawn from.</param>
/// <param name="Category">The <see cref="RegulatorCategory"/> <paramref name="PrimaryRegulator"/> maps onto.</param>
/// <param name="PrimaryFlag">The single <see cref="SafetyFlag"/> this program operationalizes, if any.</param>
/// <param name="Sop">The program's SOP requirement (<see cref="ProgramSopRequirements"/>).</param>
/// <param name="Training">The program's training requirement (<see cref="ProgramTrainingRequirements"/>).</param>
/// <param name="Permit">The program's permit requirement (<see cref="ProgramPermitRequirements"/>).</param>
/// <param name="AuditItem">The program's audit checklist item (<see cref="ProgramAuditRequirements"/>).</param>
/// <param name="EventRoutingTopic">The event-bus topic this program's events are routed to (<see cref="ProgramEventRoutingRules"/>).</param>
/// <param name="ApplicableIndustryNames">Names of every industry this program applies to (<see cref="ProgramRequirementMapping.AppliesTo"/>), in <see cref="IndustryRegistry.All"/> order.</param>
public sealed record ProgramComplianceProfile(
    string ProgramCode,
    string ProgramName,
    Regulator PrimaryRegulator,
    RegulatorCategory Category,
    SafetyFlag? PrimaryFlag,
    SopTemplate Sop,
    TrainingRequirement Training,
    PermitType Permit,
    AuditChecklistItem AuditItem,
    string EventRoutingTopic,
    IReadOnlyList<string> ApplicableIndustryNames);

/// <summary>
/// Deterministic Program -> Compliance profile mapping, computed from
/// <see cref="ProgramCatalog.All"/> and every other Safety Programs consumer.
/// </summary>
public static class ProgramComplianceProfiles
{
    /// <summary>Builds the <see cref="ProgramComplianceProfile"/> for a single program.</summary>
    public static ProgramComplianceProfile For(ProgramDefinition program) => new(
        ProgramCode: program.Code,
        ProgramName: program.Name,
        PrimaryRegulator: program.PrimaryRegulator,
        Category: program.Category,
        PrimaryFlag: program.PrimaryFlag,
        Sop: ProgramSopRequirements.For(program),
        Training: ProgramTrainingRequirements.For(program),
        Permit: ProgramPermitRequirements.For(program),
        AuditItem: ProgramAuditRequirements.For(program),
        EventRoutingTopic: ProgramEventRoutingRules.TopicFor(program),
        ApplicableIndustryNames: IndustryRegistry.All
            .Where(industry => ProgramRequirementMapping.AppliesTo(program, industry))
            .Select(industry => industry.Name)
            .ToArray());

    /// <summary>Compliance profiles for every program in <see cref="ProgramCatalog.All"/>, in catalog order.</summary>
    public static IReadOnlyList<ProgramComplianceProfile> All { get; } =
        ProgramCatalog.All.Select(For).ToArray();

    /// <summary>Compliance profiles for every program, keyed by <see cref="ProgramDefinition.Code"/>.</summary>
    public static IReadOnlyDictionary<string, ProgramComplianceProfile> ByProgramCode { get; } =
        All.ToDictionary(profile => profile.ProgramCode, StringComparer.Ordinal);

    /// <summary>Looks up a compliance profile by its exact program code.</summary>
    public static ProgramComplianceProfile? FindByProgramCode(string code) =>
        ByProgramCode.TryGetValue(code, out ProgramComplianceProfile? profile) ? profile : null;
}
