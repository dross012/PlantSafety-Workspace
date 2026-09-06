// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.Compliance
//  File:           ComplianceEvaluationCompletedIntegrationEvent.cs
//  Created:        2026-08-06
//  Description:    Versioned cross-module DTO mirroring PlantSafety.Domain.Compliance.
//                  RulesEngine.Events.ComplianceEvaluationCompletedEvent (pre-existing domain
//                  event, raised by the ComplianceEvaluation aggregate). Additive-only
//                  evolution.
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.Compliance;

/// <summary>Raised when a Compliance Rules Engine evaluation run finishes.</summary>
public sealed record ComplianceEvaluationCompletedIntegrationEvent(
    Guid ComplianceEvaluationId,
    string Outcome,
    int RulesEvaluated,
    int RulesFailed,
    string TenantId,
    DateTime CompletedAtUtc,
    Guid? ActorId,
    string ActorKind,
    Guid CorrelationId,
    Guid CausationId
) : IIntegrationEventContract
{
    /// <inheritdoc/>
    public static string EventType => "Compliance.ComplianceEvaluationCompleted";
    /// <inheritdoc/>
    public static int SchemaVersion => 1;
}
