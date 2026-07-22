// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEAssignment
//  Layer:          Domain
//  File:           PPEAssignmentFitTestRecordedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a respirator/PPE fit test result is recorded for a worker's
//                  PPEAssignment ("PPE Training & Fit Testing").
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEAssignment.Events;

/// <summary>Raised when a fit test result is recorded for a <see cref="PPEAssignment"/>.</summary>
public sealed record PPEAssignmentFitTestRecordedEvent(
    PPEAssignmentEventId EventId,
    PPEAssignmentId      PPEAssignmentId,
    bool                 Passed,
    string?              Notes,
    UtcTimestamp          TestedAtUtc,
    UtcTimestamp?         FitTestExpiresAtUtc,
    PerformedByActor      Actor,
    CorrelationId         CorrelationId,
    CausationId           CausationId
) : IPPEAssignmentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PPEAssignmentFitTestRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => TestedAtUtc;
}
