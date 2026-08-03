// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Domain
//  File:           IncidentManagementResolvedEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when an IncidentManagement aggregate transitions to Resolved via
//                  Resolve. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.Events;

/// <summary>Raised when an <see cref="Aggregates.IncidentManagement"/> aggregate is resolved.</summary>
public sealed record IncidentManagementResolvedEvent(
    IncidentManagementEventId EventId,
    IncidentManagementId      IncidentManagementId,
    string                    ResolutionNotes,
    UtcTimestamp              ResolvedAtUtc,
    PerformedByActor          Actor,
    CorrelationId             CorrelationId,
    CausationId               CausationId
) : IIncidentManagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "IncidentManagementResolved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ResolvedAtUtc;
}
