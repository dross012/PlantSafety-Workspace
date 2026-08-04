// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.DriverQualifications
//  Layer:          Domain
//  File:           DriverQualificationEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the DriverQualification lifecycle: qualification,
//                  suspension, reinstatement, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.DriverQualifications.Events;

/// <summary>Raised when a driver is qualified.</summary>
public sealed record DriverQualifiedEvent(
    DriverQualificationEventId EventId,
    DriverQualificationId DriverQualificationId,
    Guid DriverUserId,
    DriverQualificationType QualificationType,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDriverQualificationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DriverQualified";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a driver's qualification is suspended.</summary>
public sealed record DriverQualificationSuspendedEvent(
    DriverQualificationEventId EventId,
    DriverQualificationId DriverQualificationId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDriverQualificationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DriverQualificationSuspended";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a driver's qualification is reinstated.</summary>
public sealed record DriverQualificationReinstatedEvent(
    DriverQualificationEventId EventId,
    DriverQualificationId DriverQualificationId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDriverQualificationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DriverQualificationReinstated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a driver's qualification is retired.</summary>
public sealed record DriverQualificationRetiredEvent(
    DriverQualificationEventId EventId,
    DriverQualificationId DriverQualificationId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDriverQualificationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DriverQualificationRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
