// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           RiskArchivedEvent.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a Closed Risk record is archived for long-term retention.
//                  Mirrors the established Archive-event convention used by PermitWorkflow and
//                  PermitTemplate (see PermitWorkflowArchivedEvent, PermitTemplateArchivedEvent).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.RiskManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.RiskManagement.Events;

/// <summary>Raised when a Closed <see cref="Aggregates.Risk"/> is archived for long-term retention.</summary>
public sealed record RiskArchivedEvent(
    RiskEventId      EventId,
    RiskId           RiskId,
    UtcTimestamp     ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId    CorrelationId,
    CausationId      CausationId
) : IRiskDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RiskArchived";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
