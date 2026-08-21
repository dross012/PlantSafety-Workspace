// ============================================================================
//  Project:        Shared.DomainEvents.Abstractions
//  Module:         PlantSafety.Compliance
//  File:           ComplianceViolationDetectedIntegrationEvent.cs
//  Created:        2026-08-06
//  Description:    Versioned cross-module DTO mirroring PlantSafety.Domain.Compliance.
//                  RulesEngine.Events.ComplianceViolationDetectedEvent (pre-existing domain
//                  event, raised by the ComplianceEvaluation aggregate). Additive-only
//                  evolution.
// ============================================================================
namespace Shared.DomainEvents.Abstractions.PlantSafety.Compliance;

/// <summary>Raised when the Compliance Rules Engine detects a rule violation during an evaluation.</summary>
public sealed record ComplianceViolationDetectedIntegrationEvent(
    Guid ComplianceEvaluationId,
    Guid RuleId,
    string RuleCode,
    string Category,
    string ViolationCode,
    string Description,
    string Severity,
    string TargetEntityType,
    Guid TargetEntityId,
    string TenantId,
    DateTime DetectedAtUtc,
    Guid? ActorId,
    string ActorKind,
    Guid CorrelationId,
    Guid CausationId
) : IIntegrationEventContract
{
    /// <inheritdoc/>
    public static string EventType => "Compliance.ComplianceViolationDetected";
    /// <inheritdoc/>
    public static int SchemaVersion => 1;
}
