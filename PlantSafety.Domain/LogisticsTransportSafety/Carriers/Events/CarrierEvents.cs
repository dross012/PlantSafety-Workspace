// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.Carriers
//  Layer:          Domain
//  File:           CarrierEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the Carrier lifecycle: registration, qualification,
//                  suspension, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.Carriers.Events;

/// <summary>Raised when a new carrier is registered.</summary>
public sealed record CarrierRegisteredEvent(
    CarrierEventId EventId,
    CarrierId CarrierId,
    CarrierType CarrierType,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICarrierDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CarrierRegistered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a carrier is qualified to perform transport work.</summary>
public sealed record CarrierQualifiedEvent(
    CarrierEventId EventId,
    CarrierId CarrierId,
    decimal SafetyRatingScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICarrierDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CarrierQualified";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a carrier is suspended.</summary>
public sealed record CarrierSuspendedEvent(
    CarrierEventId EventId,
    CarrierId CarrierId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICarrierDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CarrierSuspended";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a carrier is retired.</summary>
public sealed record CarrierRetiredEvent(
    CarrierEventId EventId,
    CarrierId CarrierId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICarrierDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CarrierRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
