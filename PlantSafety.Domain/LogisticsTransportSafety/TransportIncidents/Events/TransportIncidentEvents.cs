// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportIncidents
//  Layer:          Domain
//  File:           TransportIncidentEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the TransportIncident lifecycle: recording, investigation
//                  start, and closure. Feeds RiskManagement and AccidentManagement via integration events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Events;

/// <summary>Raised when a new transport incident is recorded.</summary>
public sealed record TransportIncidentRecordedEvent(
    TransportIncidentEventId EventId,
    TransportIncidentId TransportIncidentId,
    TransportIncidentType IncidentType,
    TransportIncidentSeverity Severity,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITransportIncidentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TransportIncidentRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when investigation of a transport incident begins.</summary>
public sealed record TransportIncidentInvestigationStartedEvent(
    TransportIncidentEventId EventId,
    TransportIncidentId TransportIncidentId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITransportIncidentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TransportIncidentInvestigationStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a transport incident investigation is closed.</summary>
public sealed record TransportIncidentClosedEvent(
    TransportIncidentEventId EventId,
    TransportIncidentId TransportIncidentId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITransportIncidentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TransportIncidentClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
