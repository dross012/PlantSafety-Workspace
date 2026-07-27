// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.CAPA
//  Layer:          Domain
//  File:           CorrectiveActionMarkedOverdueEvent.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a CAPA CorrectiveAction passes its due date without completion.
//                  Mirrors the IX_CorrectiveAction_TenantId_Status_DueAtUtc index already present
//                  on [ContinuousImprovement].[CorrectiveAction], which exists specifically to
//                  support an overdue sweep. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ContinuousImprovement.CAPA.ValueObjects.Ids;

namespace PlantSafety.Domain.ContinuousImprovement.CAPA.Events;

/// <summary>Raised when a <see cref="Aggregates.CorrectiveAction"/> is marked Overdue.</summary>
public sealed record CorrectiveActionMarkedOverdueEvent(
    CorrectiveActionEventId EventId,
    CorrectiveActionId      CorrectiveActionId,
    UtcTimestamp            MarkedOverdueAtUtc,
    PerformedByActor        Actor,
    CorrelationId           CorrelationId,
    CausationId             CausationId
) : ICorrectiveActionDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CorrectiveActionMarkedOverdue";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => MarkedOverdueAtUtc;
}
