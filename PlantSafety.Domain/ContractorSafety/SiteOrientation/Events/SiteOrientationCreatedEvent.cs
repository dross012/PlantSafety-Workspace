// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.SiteOrientation
//  Layer:          Domain
//  File:           SiteOrientationCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real orientation lifecycle: scheduling, delivery,
//                  comprehension verification, and expiry.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.SiteOrientation.Events;

/// <summary>Raised when a new site orientation session is scheduled.</summary>
public sealed record SiteOrientationScheduledEvent(
    SiteOrientationEventId EventId,
    SiteOrientationId SiteOrientationId,
    string ContractorName,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISiteOrientationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SiteOrientationScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the orientation session is delivered.</summary>
public sealed record SiteOrientationDeliveredEvent(
    SiteOrientationEventId EventId,
    SiteOrientationId SiteOrientationId,
    int AttendeeCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISiteOrientationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SiteOrientationDelivered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when comprehension is verified and the contractor is cleared for site access.</summary>
public sealed record SiteOrientationComprehensionVerifiedEvent(
    SiteOrientationEventId EventId,
    SiteOrientationId SiteOrientationId,
    decimal ComprehensionScore,
    UtcTimestamp ExpiresAtUtc,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISiteOrientationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SiteOrientationComprehensionVerified";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the orientation expires.</summary>
public sealed record SiteOrientationExpiredEvent(
    SiteOrientationEventId EventId,
    SiteOrientationId SiteOrientationId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISiteOrientationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SiteOrientationExpired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
