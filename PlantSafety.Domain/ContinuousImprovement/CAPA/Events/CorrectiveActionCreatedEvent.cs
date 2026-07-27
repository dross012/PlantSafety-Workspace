// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.CAPA
//  Layer:          Domain
//  File:           CorrectiveActionCreatedEvent.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new CAPA CorrectiveAction is created. Mirrors the real transition
//                  performed by [ContinuousImprovement].[usp_CorrectiveAction_Create] (initial
//                  Status = N'Open'). Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ContinuousImprovement.CAPA.ValueObjects.Ids;

namespace PlantSafety.Domain.ContinuousImprovement.CAPA.Events;

/// <summary>Raised when a new <see cref="Aggregates.CorrectiveAction"/> (CAPA) item is created.</summary>
public sealed record CorrectiveActionCreatedEvent(
    CorrectiveActionEventId EventId,
    CorrectiveActionId      CorrectiveActionId,
    Guid                    SourceEventId,
    string                  SourceEventType,
    UserId                  AssignedToWorkerId,
    UtcTimestamp            DueAtUtc,
    UtcTimestamp            CreatedAtUtc,
    PerformedByActor        Actor,
    CorrelationId           CorrelationId,
    CausationId             CausationId
) : ICorrectiveActionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CorrectiveActionCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
