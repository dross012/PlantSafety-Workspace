// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEAssignment
//  Layer:          Domain
//  File:           PPEAssignmentRetiredEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a PPEAssignment is retired (worker reassigned, terminated, or PPE
//                  permanently returned).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEAssignment.Events;

/// <summary>Raised when a <see cref="PPEAssignment"/> is retired.</summary>
public sealed record PPEAssignmentRetiredEvent(
    PPEAssignmentEventId EventId,
    PPEAssignmentId      PPEAssignmentId,
    UtcTimestamp         RetiredAtUtc,
    PerformedByActor     Actor,
    CorrelationId        CorrelationId,
    CausationId          CausationId
) : IPPEAssignmentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PPEAssignmentRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RetiredAtUtc;
}
