// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.CommunicationLog
//  Layer:          Domain
//  File:           CommunicationLogEntryEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Domain event raised when a communication with an external public-safety
//                  stakeholder is logged for after-action review and compliance evidence.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Events;

/// <summary>Raised when a communication log entry is recorded.</summary>
public sealed record CommunicationLogEntryRecordedEvent(
    CommunicationLogEntryEventId EventId,
    CommunicationLogEntryId CommunicationLogEntryId,
    Guid? IncidentId,
    string DirectionCode,
    string MediumCode,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICommunicationLogEntryDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CommunicationLogEntryRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
