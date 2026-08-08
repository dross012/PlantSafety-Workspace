// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models.Industries.Facilities
//  Layer:          Abstractions
//  File:           FacilityComplianceProfile.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Compliance scorecard for a single facility: the full set of
//                  required elements (Programs, Permits, Training, Audit
//                  items, SOPs -- flattened to a uniform ComplianceElement
//                  shape) alongside which of those the facility has actually
//                  completed. "Completed" is never inferred or invented here
//                  -- it is supplied by the caller as a set of element codes
//                  (see FacilityProfileBuilder.Build's completedElementCodes
//                  parameter), since Shared.SafetyData owns no persisted
//                  Facility aggregate and has no way to know what a real
//                  facility has actually done. This type only computes the
//                  deterministic difference (Required - Completed = Missing),
//                  the resulting score, and a priority ordering -- it never
//                  fabricates completion state.
//                  Additive-only evolution.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.Consumers;

namespace Shared.SafetyData.Abstractions.Models.Industries.Facilities;

/// <summary>The kind of requirement a <see cref="ComplianceElement"/> represents.</summary>
public enum ComplianceElementKind
{
    /// <summary>A required Safety Program (<see cref="ProgramDefinition"/>).</summary>
    Program,

    /// <summary>A required permit (<see cref="PermitType"/>).</summary>
    Permit,

    /// <summary>A required training topic (<see cref="Consumers.TrainingRequirement"/>).</summary>
    TrainingRequirement,

    /// <summary>A required audit checklist item (<see cref="Consumers.AuditChecklistItem"/>).</summary>
    AuditItem,

    /// <summary>A required SOP (<see cref="Consumers.SopTemplate"/>).</summary>
    SopTemplate,
}

/// <summary>
/// A single compliance requirement, flattened to a uniform shape regardless of whether it
/// originated as a Program, Permit, Training, Audit, or SOP requirement.
/// </summary>
/// <param name="Code">Stable, machine-referenceable code (the source object's own Code, or, for <see cref="PermitType"/>, its enum name).</param>
/// <param name="Description">Human-readable description.</param>
/// <param name="Kind">Which kind of requirement this element represents.</param>
public sealed record ComplianceElement(string Code, string Description, ComplianceElementKind Kind);

/// <summary>Relative severity of a facility's current compliance gap, based on the percentage of required elements still missing.</summary>
public enum ComplianceRiskLevel
{
    /// <summary>10% or less of required elements are missing.</summary>
    Low,

    /// <summary>More than 10% but no more than 30% of required elements are missing.</summary>
    Moderate,

    /// <summary>More than 30% of required elements are missing.</summary>
    High,
}

/// <summary>
/// A single facility's compliance scorecard: required vs. completed vs. missing elements, a
/// completion score, a risk level, and the highest-priority missing elements to act on next.
/// </summary>
/// <param name="RequiredElements">Every element required for this facility's industry, in Program/Permit/Training/Audit/SOP order.</param>
/// <param name="CompletedElements">The subset of <paramref name="RequiredElements"/> the caller reported as completed.</param>
/// <param name="MissingElements">The subset of <paramref name="RequiredElements"/> not in <paramref name="CompletedElements"/>, in the same order.</param>
/// <param name="ComplianceScorePercent">CompletedElements.Count / RequiredElements.Count * 100 (100 if there are no required elements).</param>
/// <param name="RiskLevel">Derived from the percentage of <paramref name="RequiredElements"/> that are missing.</param>
/// <param name="PriorityActions">Up to the ten highest-priority missing elements, in Program &gt; Permit &gt; Training &gt; Audit &gt; SOP order.</param>
public sealed record FacilityComplianceProfile(
    IReadOnlyList<ComplianceElement> RequiredElements,
    IReadOnlyList<ComplianceElement> CompletedElements,
    IReadOnlyList<ComplianceElement> MissingElements,
    double ComplianceScorePercent,
    ComplianceRiskLevel RiskLevel,
    IReadOnlyList<ComplianceElement> PriorityActions);

/// <summary>
/// Builds the flattened <see cref="ComplianceElement"/> list for a set of applicable programs,
/// and the resulting <see cref="FacilityComplianceProfile"/> once completion state is supplied.
/// </summary>
public static class FacilityComplianceProfileBuilder
{
    private const int MaxPriorityActions = 10;

    /// <summary>
    /// Flattens the given programs' own Program/Permit/Training/Audit/SOP requirements into a
    /// single ordered <see cref="ComplianceElement"/> list (one Program element per program,
    /// one Permit element per distinct <see cref="PermitType"/>, one Training/Audit/SOP element
    /// per program). This is the industry-driven "what's required" list, independent of any
    /// specific facility's completion state.
    /// </summary>
    public static IReadOnlyList<ComplianceElement> BuildRequiredElements(IReadOnlyList<ProgramDefinition> programs)
    {
        var elements = new List<ComplianceElement>();

        elements.AddRange(programs.Select(program => new ComplianceElement(program.Code, program.Name, ComplianceElementKind.Program)));

        elements.AddRange(programs
            .Select(Consumers.ProgramPermitRequirements.For)
            .Distinct()
            .Select(permit => new ComplianceElement(permit.ToString(), $"{permit} permit", ComplianceElementKind.Permit)));

        elements.AddRange(programs.Select(program =>
        {
            Consumers.TrainingRequirement training = Consumers.ProgramTrainingRequirements.For(program);
            return new ComplianceElement(training.Code, training.Title, ComplianceElementKind.TrainingRequirement);
        }));

        elements.AddRange(programs.Select(program =>
        {
            Consumers.AuditChecklistItem auditItem = Consumers.ProgramAuditRequirements.For(program);
            return new ComplianceElement(auditItem.Code, auditItem.Description, ComplianceElementKind.AuditItem);
        }));

        elements.AddRange(programs.Select(program =>
        {
            Consumers.SopTemplate sop = Consumers.ProgramSopRequirements.For(program);
            return new ComplianceElement(sop.Code, sop.Title, ComplianceElementKind.SopTemplate);
        }));

        return elements;
    }

    /// <summary>
    /// Builds the full <see cref="FacilityComplianceProfile"/> for the given required elements,
    /// given the caller-supplied set of completed element codes.
    /// </summary>
    public static FacilityComplianceProfile Build(IReadOnlyList<ComplianceElement> requiredElements, IReadOnlySet<string> completedElementCodes)
    {
        IReadOnlyList<ComplianceElement> completed = requiredElements
            .Where(element => completedElementCodes.Contains(element.Code))
            .ToArray();

        IReadOnlyList<ComplianceElement> missing = requiredElements
            .Where(element => !completedElementCodes.Contains(element.Code))
            .ToArray();

        double scorePercent = requiredElements.Count == 0
            ? 100d
            : (double)completed.Count / requiredElements.Count * 100d;

        double missingPercent = requiredElements.Count == 0 ? 0d : (double)missing.Count / requiredElements.Count * 100d;

        ComplianceRiskLevel riskLevel = missingPercent switch
        {
            <= 10d => ComplianceRiskLevel.Low,
            <= 30d => ComplianceRiskLevel.Moderate,
            _ => ComplianceRiskLevel.High,
        };

        IReadOnlyList<ComplianceElement> priorityActions = missing.Take(MaxPriorityActions).ToArray();

        return new FacilityComplianceProfile(requiredElements, completed, missing, scorePercent, riskLevel, priorityActions);
    }
}
