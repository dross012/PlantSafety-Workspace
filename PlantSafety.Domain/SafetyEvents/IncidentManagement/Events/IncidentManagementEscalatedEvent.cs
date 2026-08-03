// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Domain
//  File:           IncidentManagementEscalatedEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when an IncidentManagement aggregate is escalated (severity bump plus
//                  supervisor-notification reason). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.Events;

/// <summary>Raised when an <see cref="Aggregates.IncidentManagement"/> aggregate is escalated.</summary>
public sealed record IncidentManagementEscalatedEvent(
    IncidentManagementEventId EventId,
    IncidentManagementId      IncidentManagementId,
    string                    Reason,
    string                    PreviousSeverity,
    string                    NewSeverity,
    int                       EscalationCount,
    UtcTimestamp              EscalatedAtUtc,
    PerformedByActor          Actor,
    CorrelationId             CorrelationId,
    CausationId               CausationId
) : IIncidentManagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "IncidentManagementEscalated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => EscalatedAtUtc;
}
