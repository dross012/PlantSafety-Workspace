// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.CAPA
//  Layer:          Domain
//  File:           CorrectiveActionClosedEvent.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a CAPA CorrectiveAction is verified and formally closed (terminal
//                  state). NOTE: the pre-existing Application-layer integration event of the same
//                  name (PlantSafety.Application.Features.ContinuousImprovement.Events.CAPA.
//                  CorrectiveActionClosedEvent) declares ClosedByActorId/ClosureNotes fields that
//                  have no backing column on [ContinuousImprovement].[CorrectiveAction] (only
//                  CompletedAtUtc exists) and is never published by any handler today -- this
//                  Domain-layer event intentionally only carries what the schema actually
//                  supports. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ContinuousImprovement.CAPA.ValueObjects.Ids;

namespace PlantSafety.Domain.ContinuousImprovement.CAPA.Events;

/// <summary>Raised when a <see cref="Aggregates.CorrectiveAction"/> is verified and formally closed.</summary>
public sealed record CorrectiveActionClosedEvent(
    CorrectiveActionEventId EventId,
    CorrectiveActionId      CorrectiveActionId,
    UtcTimestamp            ClosedAtUtc,
    PerformedByActor        Actor,
    CorrelationId           CorrelationId,
    CausationId             CausationId
) : ICorrectiveActionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CorrectiveActionClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}
