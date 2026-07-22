// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEAssignment
//  Layer:          Domain
//  File:           PPEAssignmentFlaggedForReplacementEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when an assigned worker's PPE is flagged for replacement (failed fit
//                  test, damage, or approaching expiration).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEAssignment.Events;

/// <summary>Raised when a <see cref="PPEAssignment"/> is flagged for replacement.</summary>
public sealed record PPEAssignmentFlaggedForReplacementEvent(
    PPEAssignmentEventId EventId,
    PPEAssignmentId      PPEAssignmentId,
    string               Reason,
    UtcTimestamp         FlaggedAtUtc,
    PerformedByActor     Actor,
    CorrelationId        CorrelationId,
    CausationId          CausationId
) : IPPEAssignmentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PPEAssignmentFlaggedForReplacement";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => FlaggedAtUtc;
}
