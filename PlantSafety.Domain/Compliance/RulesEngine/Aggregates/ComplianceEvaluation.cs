// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           ComplianceEvaluation.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root representing a single deterministic run of the Compliance
//                  Rules Engine (Handbook Phase 5 §6.3) against a target entity. The Application
//                  layer's ComplianceEvaluator invokes each registered IComplianceRule and feeds
//                  the pass/fail outcome back into this aggregate via RecordRuleResult; the
//                  aggregate is the sole authority for the overall Compliant/NonCompliant
//                  determination (via ComplianceOutcomePolicy) and for raising
//                  ComplianceViolationDetectedEvent per failing rule so downstream consumers
//                  (notably AuditTimeline, Phase 5 §6.1) observe every violation deterministically.
//                  Additive-only evolution.
// ============================================================================
using System.Collections.ObjectModel;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.RulesEngine.Entities;
using PlantSafety.Domain.Compliance.RulesEngine.Enums;
using PlantSafety.Domain.Compliance.RulesEngine.Events;
using PlantSafety.Domain.Compliance.RulesEngine.Exceptions;
using PlantSafety.Domain.Compliance.RulesEngine.Policies;
using PlantSafety.Domain.Compliance.RulesEngine.ValueObjects;
using PlantSafety.Domain.Compliance.RulesEngine.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.RulesEngine.Aggregates;

/// <summary>Aggregate root for a single Compliance Rules Engine evaluation run.</summary>
public sealed class ComplianceEvaluation
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this evaluation.</summary>
    public ComplianceEvaluationId ComplianceEvaluationId { get; private set; }

    /// <summary>Tenant that owns this evaluation.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site scope, or null for tenant-wide.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Type discriminator of the entity that was evaluated (e.g. "Risk", "Permit", "AuditFinding").</summary>
    public string TargetEntityType { get; private set; } = string.Empty;

    /// <summary>Identity of the entity that was evaluated.</summary>
    public Guid TargetEntityId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ComplianceEvaluationStatus Status { get; private set; } = ComplianceEvaluationStatus.Default;

    private readonly List<ComplianceRuleResult> _ruleResults = [];

    /// <summary>Rule results recorded so far, in evaluation order.</summary>
    public IReadOnlyList<ComplianceRuleResult> RuleResults => _ruleResults.AsReadOnly();

    /// <summary>UTC instant this evaluation run started.</summary>
    public UtcTimestamp StartedAtUtc { get; private set; }

    /// <summary>UTC instant this evaluation run completed, if any.</summary>
    public UtcTimestamp? CompletedAtUtc { get; private set; }

    /// <summary>Identity of the user or system actor who triggered this evaluation.</summary>
    public UserId TriggeredBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IComplianceRuleEngineDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IComplianceRuleEngineDomainEvent> DomainEvents
        => new ReadOnlyCollection<IComplianceRuleEngineDomainEvent>(_domainEvents);

    // For rehydration only.
    private ComplianceEvaluation() { }

    /// <summary>Starts a new ComplianceEvaluation run and raises <see cref="ComplianceEvaluationStartedEvent"/>.</summary>
    public static ComplianceEvaluation Start(
        Guid tenantId,
        Guid? siteId,
        string targetEntityType,
        Guid targetEntityId,
        UserId triggeredBy,
        UtcTimestamp startedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        var agg = new ComplianceEvaluation
        {
            ComplianceEvaluationId = ComplianceEvaluationId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            TargetEntityType = (targetEntityType ?? string.Empty).Trim(),
            TargetEntityId = targetEntityId,
            Status = ComplianceEvaluationStatus.InProgress,
            StartedAtUtc = startedAtUtc,
            TriggeredBy = triggeredBy,
        };

        agg.AddDomainEvent(new ComplianceEvaluationStartedEvent(
            Uuid7.New(), agg.ComplianceEvaluationId, agg.TargetEntityType, agg.TargetEntityId,
            startedAtUtc, actor, correlationId, causationId));

        return agg;
    }

    /// <summary>
    /// Records the outcome of one rule invocation. When the rule failed, raises a
    /// <see cref="ComplianceViolationDetectedEvent"/> per violation detail so no violation is
    /// ever silently dropped.
    /// </summary>
    public void RecordRuleResult(
        ComplianceRuleId ruleId, string ruleCode, string ruleName, ComplianceRuleCategory category,
        bool passed, IReadOnlyList<ComplianceViolation> violations,
        UtcTimestamp evaluatedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != ComplianceEvaluationStatus.InProgress)
            throw new ComplianceEvaluationInvalidStateException(ComplianceEvaluationId, Status.Code, "RecordRuleResult", correlationId, causationId);

        var result = passed
            ? ComplianceRuleResult.Pass(ComplianceEvaluationId, ruleId, ruleCode, ruleName, category, evaluatedAtUtc)
            : ComplianceRuleResult.Fail(ComplianceEvaluationId, ruleId, ruleCode, ruleName, category, evaluatedAtUtc, violations);

        _ruleResults.Add(result);

        if (!passed)
        {
            foreach (var violation in violations)
            {
                AddDomainEvent(new ComplianceViolationDetectedEvent(
                    Uuid7.New(), ComplianceEvaluationId, ruleId, ruleCode, category,
                    violation.ViolationCode, violation.Description, violation.Severity,
                    TargetEntityType, TargetEntityId, evaluatedAtUtc, actor, correlationId, causationId));
            }
        }
    }

    /// <summary>
    /// Completes the evaluation run, deriving the overall Compliant/NonCompliant outcome via
    /// <see cref="ComplianceOutcomePolicy"/> and raising <see cref="ComplianceEvaluationCompletedEvent"/>.
    /// </summary>
    public ComplianceEvaluationStatus Complete(UtcTimestamp completedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != ComplianceEvaluationStatus.InProgress)
            throw new ComplianceEvaluationInvalidStateException(ComplianceEvaluationId, Status.Code, "Complete", correlationId, causationId);

        Status = ComplianceOutcomePolicy.DetermineOutcome(_ruleResults);
        CompletedAtUtc = completedAtUtc;

        var rulesFailed = _ruleResults.Count(r => !r.Passed);

        AddDomainEvent(new ComplianceEvaluationCompletedEvent(
            Uuid7.New(), ComplianceEvaluationId, Status, _ruleResults.Count, rulesFailed,
            completedAtUtc, actor, correlationId, causationId));

        return Status;
    }

    /// <summary>All violations across all rule results, ordered most-severe first.</summary>
    public IReadOnlyList<ComplianceViolation> AllViolations()
        => _ruleResults.SelectMany(r => r.Violations).OrderByDescending(v => v.Severity.Rank).ToList();

    private void AddDomainEvent(IComplianceRuleEngineDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
