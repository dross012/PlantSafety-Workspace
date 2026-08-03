// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: API/read-model DTO for a single rule result recorded within a
//   ComplianceEvaluation.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.RulesEngine.Models;

/// <summary>Read-model DTO for a single compliance rule result.</summary>
public sealed record ComplianceRuleResultDto
{
    /// <summary>Stable machine-readable rule code.</summary>
    public required string RuleCode { get; init; }

    /// <summary>Human-readable rule name.</summary>
    public required string RuleName { get; init; }

    /// <summary>Governing category (Safety/Regulatory/InternalPolicy).</summary>
    public required string Category { get; init; }

    /// <summary>Whether the rule passed.</summary>
    public required bool Passed { get; init; }

    /// <summary>Violation details raised by this rule; empty when <see cref="Passed"/> is <see langword="true"/>.</summary>
    public required IReadOnlyList<ComplianceViolationDto> Violations { get; init; }

    /// <summary>UTC instant the rule was evaluated.</summary>
    public required DateTime EvaluatedAtUtc { get; init; }
}
