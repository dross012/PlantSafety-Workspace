// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEAssignment
//  Layer:          Domain
//  File:           PPEAssignmentIssuedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when PPE is physically issued to the assigned worker.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEAssignment.Events;

/// <summary>Raised when PPE is physically issued to the worker for a <see cref="PPEAssignment"/>.</summary>
public sealed record PPEAssignmentIssuedEvent(
    PPEAssignmentEventId EventId,
    PPEAssignmentId      PPEAssignmentId,
    UtcTimestamp         IssuedAtUtc,
    PerformedByActor     Actor,
    CorrelationId        CorrelationId,
    CausationId          CausationId
) : IPPEAssignmentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PPEAssignmentIssued";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => IssuedAtUtc;
}
