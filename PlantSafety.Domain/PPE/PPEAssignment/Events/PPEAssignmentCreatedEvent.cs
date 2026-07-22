// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEAssignment
//  Layer:          Domain
//  File:           PPEAssignmentCreatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new PPEAssignment aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEAssignment.Events;

/// <summary>Raised when a new <see cref="PPEAssignment"/> aggregate is created.</summary>
public sealed record PPEAssignmentCreatedEvent(
    PPEAssignmentEventId EventId,
    PPEAssignmentId      PPEAssignmentId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IPPEAssignmentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PPEAssignmentCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}