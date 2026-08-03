// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Explicit, deterministic static mapping between the ComplianceEvaluation
//   domain aggregate and its Application-layer DTOs. This codebase does not use Mapster or any
//   other reflection-based mapper anywhere in the Application layer (verified: no Mapster
//   PackageReference on PlantSafety.Application) and Handbook §2.16C explicitly prohibits
//   reflection-based/AutoMapper-style mapping; explicit static mapper classes are the existing,
//   established convention (see e.g. Infrastructure row mappers), so this class follows that
//   convention rather than introducing a new reflection-mapping dependency.
// SchemaVersion: 1

using PlantSafety.Domain.Compliance.RulesEngine.Aggregates;
using PlantSafety.Domain.Compliance.RulesEngine.Entities;
using PlantSafety.Application.Compliance.RulesEngine.Models;

namespace PlantSafety.Application.Compliance.RulesEngine.Mapping;

/// <summary>Explicit static mapper between ComplianceEvaluation domain types and Application DTOs.</summary>
public static class ComplianceEvaluationMapper
{
    /// <summary>Maps a <see cref="ComplianceEvaluation"/> aggregate to its DTO representation.</summary>
    public static ComplianceEvaluationDto ToDto(ComplianceEvaluation evaluation)
        => new()
        {
            ComplianceEvaluationId = evaluation.ComplianceEvaluationId.Value,
            TenantId = evaluation.TenantId,
            SiteId = evaluation.SiteId,
            TargetEntityType = evaluation.TargetEntityType,
            TargetEntityId = evaluation.TargetEntityId,
            Status = evaluation.Status.Code,
            RuleResults = evaluation.RuleResults.Select(ToDto).ToList(),
            StartedAtUtc = evaluation.StartedAtUtc.Value,
            CompletedAtUtc = evaluation.CompletedAtUtc?.Value,
            TriggeredBy = evaluation.TriggeredBy.Value,
        };

    /// <summary>Maps a <see cref="ComplianceRuleResult"/> entity to its DTO representation.</summary>
    public static ComplianceRuleResultDto ToDto(ComplianceRuleResult result)
        => new()
        {
            RuleCode = result.RuleCode,
            RuleName = result.RuleName,
            Category = result.Category.Code,
            Passed = result.Passed,
            Violations = result.Violations.Select(v => new ComplianceViolationDto
            {
                ViolationCode = v.ViolationCode,
                Description = v.Description,
                Severity = v.Severity.Code,
                RemediationGuidance = v.RemediationGuidance,
            }).ToList(),
            EvaluatedAtUtc = result.EvaluatedAtUtc.Value,
        };
}
