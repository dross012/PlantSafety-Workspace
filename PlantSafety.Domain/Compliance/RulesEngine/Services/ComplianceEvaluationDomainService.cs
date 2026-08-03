// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           ComplianceEvaluationDomainService.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain service coordinating cross-aggregate compliance summarization logic
//                  that does not belong on a single ComplianceEvaluation instance (e.g. rolling up
//                  the most severe outstanding violation across the evaluation history of a
//                  target entity). Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Common.Services;
using PlantSafety.Domain.Compliance.RulesEngine.Aggregates;
using PlantSafety.Domain.Compliance.RulesEngine.ValueObjects;

namespace PlantSafety.Domain.Compliance.RulesEngine.Services;

/// <summary>Domain service for cross-evaluation compliance summarization logic.</summary>
public sealed class ComplianceEvaluationDomainService : IDomainService
{
    /// <summary>
    /// Returns the single most severe unresolved violation across a set of ComplianceEvaluation
    /// runs for the same target entity, or <see langword="null"/> when none failed.
    /// </summary>
    public ComplianceViolation? MostSevereOutstandingViolation(IEnumerable<ComplianceEvaluation> evaluationHistory)
        => evaluationHistory
            .OrderByDescending(e => e.CompletedAtUtc?.Value ?? e.StartedAtUtc.Value)
            .SelectMany(e => e.AllViolations())
            .OrderByDescending(v => v.Severity.Rank)
            .FirstOrDefault();
}
