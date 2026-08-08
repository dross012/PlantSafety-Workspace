// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Flat read-model DTO for a single Compliance.StandardsRiskAssessmentRule
//   row -- a risk-assessment methodology rule tied to a parent StandardsRequirement.
//   HazardIdentificationSteps/RiskReductionMeasures are deserialized from the underlying
//   JSON array string columns into ordered string lists. Global reference data, not
//   tenant-scoped.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Standards.Models;

/// <summary>Flat read-model DTO for a single Compliance.StandardsRiskAssessmentRule row.</summary>
public sealed record RiskAssessmentRuleDto
{
    /// <summary>Surrogate identity.</summary>
    public required Guid RiskAssessmentRuleId { get; init; }

    /// <summary>Identity of the parent standards requirement.</summary>
    public required Guid RequirementId { get; init; }

    /// <summary>Risk assessment methodology (e.g. "ISO 12100", "ANSI B11.0").</summary>
    public required string Methodology { get; init; }

    /// <summary>Ordered hazard-identification steps for this methodology.</summary>
    public required IReadOnlyList<string> HazardIdentificationSteps { get; init; }

    /// <summary>Ordered risk-reduction measures for this methodology.</summary>
    public required IReadOnlyList<string> RiskReductionMeasures { get; init; }

    /// <summary>Narrative description of the rule.</summary>
    public string? Description { get; init; }

    /// <summary>When this row was created.</summary>
    public required DateTime CreatedOn { get; init; }
}
