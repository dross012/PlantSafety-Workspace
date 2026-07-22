// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitWorkflow
//  Layer:          Domain
//  File:           PermitWorkflowControlsImplementedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when required safety controls (LOTO, PPE, barricades, atmospheric
//                  testing) have been implemented for a PermitWorkflow (PTW Workflow Step 3).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitWorkflow.Events;

/// <summary>Raised when required safety controls have been implemented for a <see cref="PermitWorkflow"/>.</summary>
public sealed record PermitWorkflowControlsImplementedEvent(
    PermitWorkflowEventId EventId,
    PermitWorkflowId      PermitWorkflowId,
    string                ControlsConfirmedBy,
    UtcTimestamp          ImplementedAtUtc,
    PerformedByActor      Actor,
    CorrelationId         CorrelationId,
    CausationId           CausationId
) : IPermitWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitWorkflowControlsImplemented";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ImplementedAtUtc;
}
