// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           AccidentArchivedEvent.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a Closed Accident record is archived for long-term retention.
//                  Mirrors the established Archive-event convention used by PermitWorkflow and
//                  PermitTemplate (see PermitWorkflowArchivedEvent, PermitTemplateArchivedEvent).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.AccidentManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.AccidentManagement.Events;

/// <summary>Raised when a Closed <see cref="Aggregates.Accident"/> is archived for long-term retention.</summary>
public sealed record AccidentArchivedEvent(
    AccidentEventId  EventId,
    AccidentId       AccidentId,
    UtcTimestamp     ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IAccidentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccidentArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
