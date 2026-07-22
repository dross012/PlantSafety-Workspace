// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitWorkflow
//  Layer:          Domain
//  File:           PermitWorkflowCreatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new PermitWorkflow aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitWorkflow.Events;

/// <summary>Raised when a new <see cref="PermitWorkflow"/> aggregate is created.</summary>
public sealed record PermitWorkflowCreatedEvent(
    PermitWorkflowEventId EventId,
    PermitWorkflowId      PermitWorkflowId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IPermitWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitWorkflowCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}