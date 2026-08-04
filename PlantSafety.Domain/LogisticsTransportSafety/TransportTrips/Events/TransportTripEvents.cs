// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportTrips
//  Layer:          Domain
//  File:           TransportTripEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the TransportTrip lifecycle: planning, completion, and
//                  cancellation.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Events;

/// <summary>Raised when a new transport trip is planned.</summary>
public sealed record TransportTripPlannedEvent(
    TransportTripEventId EventId,
    TransportTripId TransportTripId,
    TransportTripType TripType,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITransportTripDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TransportTripPlanned";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a transport trip is completed.</summary>
public sealed record TransportTripCompletedEvent(
    TransportTripEventId EventId,
    TransportTripId TransportTripId,
    decimal DistanceMiles,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITransportTripDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TransportTripCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a transport trip is cancelled.</summary>
public sealed record TransportTripCancelledEvent(
    TransportTripEventId EventId,
    TransportTripId TransportTripId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ITransportTripDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "TransportTripCancelled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
