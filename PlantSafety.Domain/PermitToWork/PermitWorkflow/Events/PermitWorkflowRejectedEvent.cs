// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitWorkflow
//  Layer:          Domain
//  File:           PermitWorkflowRejectedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a PermitWorkflow is declined during hazard assessment,
//                  control implementation, or authorization review.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitWorkflow.Events;

/// <summary>Raised when a <see cref="PermitWorkflow"/> request is declined.</summary>
public sealed record PermitWorkflowRejectedEvent(
    PermitWorkflowEventId EventId,
    PermitWorkflowId      PermitWorkflowId,
    UserId                RejectedBy,
    string                Reason,
    UtcTimestamp          RejectedAtUtc,
    PerformedByActor      Actor,
    CorrelationId         CorrelationId,
    CausationId           CausationId
) : IPermitWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitWorkflowRejected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RejectedAtUtc;
}
