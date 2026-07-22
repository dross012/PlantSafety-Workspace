// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitWorkflow
//  Layer:          Domain
//  File:           PermitWorkflowClosedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when the work area has been inspected, LOTO removed, and the
//                  closure signature recorded, closing the permit (PTW Workflow Step 6).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitWorkflow.Events;

/// <summary>Raised when a <see cref="PermitWorkflow"/> is closed with a recorded closure signature.</summary>
public sealed record PermitWorkflowClosedEvent(
    PermitWorkflowEventId EventId,
    PermitWorkflowId      PermitWorkflowId,
    UserId                ClosureSignedBy,
    string?               ClosureNotes,
    UtcTimestamp          ClosedAtUtc,
    PerformedByActor      Actor,
    CorrelationId         CorrelationId,
    CausationId           CausationId
) : IPermitWorkflowDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitWorkflowClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}
