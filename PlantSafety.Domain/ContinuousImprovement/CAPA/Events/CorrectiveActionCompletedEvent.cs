// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.CAPA
//  Layer:          Domain
//  File:           CorrectiveActionCompletedEvent.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a CAPA CorrectiveAction's work is completed and it moves to
//                  AwaitingVerification. Mirrors the CompletedAtUtc column already present on
//                  [ContinuousImprovement].[CorrectiveAction]. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ContinuousImprovement.CAPA.ValueObjects.Ids;

namespace PlantSafety.Domain.ContinuousImprovement.CAPA.Events;

/// <summary>Raised when a <see cref="Aggregates.CorrectiveAction"/>'s work is completed (awaiting verification).</summary>
public sealed record CorrectiveActionCompletedEvent(
    CorrectiveActionEventId EventId,
    CorrectiveActionId      CorrectiveActionId,
    UtcTimestamp            CompletedAtUtc,
    PerformedByActor        Actor,
    CorrelationId           CorrelationId,
    CausationId             CausationId
) : ICorrectiveActionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CorrectiveActionCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CompletedAtUtc;
}
