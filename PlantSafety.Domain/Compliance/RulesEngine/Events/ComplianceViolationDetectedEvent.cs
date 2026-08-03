// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           ComplianceViolationDetectedEvent.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised once per failing ComplianceRuleResult recorded during a
//                  ComplianceEvaluation. Consumed by AuditTimeline (Phase 5 §6.1) so violations
//                  appear in the reconstructed chronological view. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.RulesEngine.Enums;
using PlantSafety.Domain.Compliance.RulesEngine.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.RulesEngine.Events;

/// <summary>Raised when a compliance rule fails and records one or more violations.</summary>
public sealed record ComplianceViolationDetectedEvent(
    Guid EventId,
    ComplianceEvaluationId ComplianceEvaluationId,
    ComplianceRuleId RuleId,
    string RuleCode,
    ComplianceRuleCategory Category,
    string ViolationCode,
    string Description,
    ComplianceSeverity Severity,
    string TargetEntityType,
    Guid TargetEntityId,
    UtcTimestamp DetectedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IComplianceRuleEngineDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ComplianceViolationDetected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DetectedAtUtc;
}
