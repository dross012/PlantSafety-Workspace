// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           ComplianceEvaluationCompletedEvent.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a ComplianceEvaluation run finishes, carrying the final
//                  Compliant/NonCompliant outcome. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.RulesEngine.Enums;
using PlantSafety.Domain.Compliance.RulesEngine.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.RulesEngine.Events;

/// <summary>Raised when a ComplianceEvaluation run completes.</summary>
public sealed record ComplianceEvaluationCompletedEvent(
    Guid EventId,
    ComplianceEvaluationId ComplianceEvaluationId,
    ComplianceEvaluationStatus Outcome,
    int RulesEvaluated,
    int RulesFailed,
    UtcTimestamp CompletedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IComplianceRuleEngineDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ComplianceEvaluationCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}
