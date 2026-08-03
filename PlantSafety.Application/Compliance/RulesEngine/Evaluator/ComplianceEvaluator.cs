// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Aggregates every registered IComplianceRule into a single deterministic
//   pipeline run. For a given ComplianceRuleContext, invokes all rules whose Category is included
//   in the request (defaulting to all three: Safety, Regulatory, InternalPolicy), records each
//   outcome onto a ComplianceEvaluation aggregate, and completes the evaluation. This is the
//   Application-layer orchestration counterpart to Predictive Scheduling's rule engine
//   (PlantSafety.Application.Scheduling) but is fully self-contained under Compliance and does
//   not depend on Scheduling in any way.
// SchemaVersion: 1

using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.RulesEngine.Aggregates;
using PlantSafety.Domain.Compliance.RulesEngine.Enums;
using PlantSafety.Domain.Compliance.RulesEngine.Interfaces;
using PlantSafety.Domain.Compliance.RulesEngine.ValueObjects;
using PlantSafety.Application.Compliance.RulesEngine.Rules;

namespace PlantSafety.Application.Compliance.RulesEngine.Evaluator;

/// <summary>Orchestrates a full Compliance Rules Engine evaluation run across all registered rules.</summary>
public sealed class ComplianceEvaluator
{
    private readonly IReadOnlyList<IComplianceRule> _rules;
    private readonly IComplianceEvaluationFactory _factory;

    /// <summary>Initialises the evaluator with every registered <see cref="IComplianceRule"/> (via DI collection injection).</summary>
    public ComplianceEvaluator(IEnumerable<IComplianceRule> rules, IComplianceEvaluationFactory factory)
    {
        _rules = rules.ToList();
        _factory = factory;
    }

    /// <summary>
    /// Runs every registered rule whose <see cref="IComplianceRule.Category"/> is present in
    /// <paramref name="categories"/> (or every rule, when <paramref name="categories"/> is
    /// <see langword="null"/>) against <paramref name="context"/>, recording each outcome on a
    /// new <see cref="ComplianceEvaluation"/> aggregate and completing it.
    /// </summary>
    public ComplianceEvaluation Evaluate(
        ComplianceRuleContext context,
        UserId triggeredBy,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        IReadOnlyCollection<ComplianceRuleCategory>? categories = null)
    {
        var nowUtc = UtcTimestamp.From(context.EvaluatedAtUtc);

        var evaluation = _factory.Start(
            context.TenantId, context.SiteId, context.TargetEntityType, context.TargetEntityId,
            triggeredBy, nowUtc, actor, correlationId, causationId);

        var applicableRules = categories is null
            ? _rules
            : _rules.Where(r => categories.Contains(r.Category)).ToList();

        foreach (var rule in applicableRules.OrderBy(r => r.Category.Code, StringComparer.Ordinal).ThenBy(r => r.RuleCode, StringComparer.Ordinal))
        {
            var outcome = rule.Evaluate(context);

            var violations = outcome.Violations
                .Select(v => ComplianceViolation.Create(v.ViolationCode, v.Description, v.Severity, v.RemediationGuidance))
                .ToList();

            evaluation.RecordRuleResult(
                Domain.Compliance.RulesEngine.ValueObjects.Ids.ComplianceRuleId.New(),
                rule.RuleCode, rule.RuleName, rule.Category,
                outcome.Passed, violations, nowUtc, actor, correlationId, causationId);
        }

        evaluation.Complete(nowUtc, actor, correlationId, causationId);

        return evaluation;
    }
}
