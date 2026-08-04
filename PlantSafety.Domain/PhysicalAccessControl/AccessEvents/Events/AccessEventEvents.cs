// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessEvents
//  Layer:          Domain
//  File:           AccessEventEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the AccessEvent lifecycle: ingestion,
//                  processing, and anomaly flagging.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Events;

/// <summary>Raised when a raw badge-system event is ingested from an external webhook/IoT feed.</summary>
public sealed record AccessEventIngestedEvent(
    AccessEventEventId EventId,
    AccessEventId AccessEventId,
    Guid AccessPointId,
    Guid? CredentialId,
    AccessEventType RawEventType,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAccessEventDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccessEventIngested";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an access event has been correlated against zone occupancy / anomaly detection.</summary>
public sealed record AccessEventProcessedEvent(
    AccessEventEventId EventId,
    AccessEventId AccessEventId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAccessEventDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccessEventProcessed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an access event is flagged as anomalous.</summary>
public sealed record AccessEventFlaggedEvent(
    AccessEventEventId EventId,
    AccessEventId AccessEventId,
    string AnomalyType,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAccessEventDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccessEventFlagged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
