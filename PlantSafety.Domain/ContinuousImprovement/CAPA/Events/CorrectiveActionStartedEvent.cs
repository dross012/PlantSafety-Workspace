// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.CAPA
//  Layer:          Domain
//  File:           CorrectiveActionStartedEvent.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a CAPA CorrectiveAction transitions Open -> InProgress.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ContinuousImprovement.CAPA.ValueObjects.Ids;

namespace PlantSafety.Domain.ContinuousImprovement.CAPA.Events;

/// <summary>Raised when a <see cref="Aggregates.CorrectiveAction"/> transitions from Open to InProgress.</summary>
public sealed record CorrectiveActionStartedEvent(
    CorrectiveActionEventId EventId,
    CorrectiveActionId      CorrectiveActionId,
    UtcTimestamp            StartedAtUtc,
    PerformedByActor        Actor,
    CorrelationId           CorrelationId,
    CausationId             CausationId
) : ICorrectiveActionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CorrectiveActionStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => StartedAtUtc;
}
