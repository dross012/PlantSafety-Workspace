// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditTimeline
//  Layer:          Domain
//  File:           ConcurrencyConflictRecordedEvent.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when an optimistic concurrency conflict is captured rather than
//                  silently dropped. Consumed by AuditTimeline reconstruction (§6.1) to surface
//                  concurrency-conflict history alongside events, state changes, and scheduling
//                  decisions. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Auditing.AuditTimeline.ValueObjects.Ids;

namespace PlantSafety.Domain.Auditing.AuditTimeline.Events;

/// <summary>Raised when an optimistic concurrency conflict is captured.</summary>
public sealed record ConcurrencyConflictRecordedEvent(
    Guid EventId,
    ConcurrencyConflictRecordId ConcurrencyConflictRecordId,
    string EntityType,
    Guid EntityId,
    string ExpectedETag,
    string? ActualETag,
    string AttemptedOperation,
    UtcTimestamp OccurredAtUtcValue,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAuditTimelineDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ConcurrencyConflictRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => OccurredAtUtcValue;
}
