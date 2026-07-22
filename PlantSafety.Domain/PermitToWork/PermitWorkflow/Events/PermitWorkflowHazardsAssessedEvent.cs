// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitWorkflow
//  Layer:          Domain
//  File:           PermitWorkflowHazardsAssessedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when the safety officer completes the hazard assessment for a
//                  PermitWorkflow (PTW Workflow Step 2).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitWorkflow.Events;

/// <summary>Raised when the hazard assessment for a <see cref="PermitWorkflow"/> is completed.</summary>
public sealed record PermitWorkflowHazardsAssessedEvent(
    PermitWorkflowEventId EventId,
    PermitWorkflowId      PermitWorkflowId,
    string                RequiredControls,
    int                   RiskScore,
    UtcTimestamp          AssessedAtUtc,
    PerformedByActor      Actor,
    CorrelationId         CorrelationId,
    CausationId           CausationId
) : IPermitWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitWorkflowHazardsAssessed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AssessedAtUtc;
}
