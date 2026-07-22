// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PreQualificationVetting
//  Layer:          Domain
//  File:           PreQualificationVettingCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real vetting lifecycle: submission, review start,
//                  approval, rejection, and expiry.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Events;

/// <summary>Raised when a contractor is submitted for pre-qualification review.</summary>
public sealed record PreQualificationVettingSubmittedEvent(
    PreQualificationVettingEventId EventId,
    PreQualificationVettingId PreQualificationVettingId,
    string ContractorName,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPreQualificationVettingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PreQualificationVettingSubmitted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the vetting review begins.</summary>
public sealed record PreQualificationVettingReviewStartedEvent(
    PreQualificationVettingEventId EventId,
    PreQualificationVettingId PreQualificationVettingId,
    UserId ReviewedByUserId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPreQualificationVettingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PreQualificationVettingReviewStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the contractor is approved to work at the facility.</summary>
public sealed record PreQualificationVettingApprovedEvent(
    PreQualificationVettingEventId EventId,
    PreQualificationVettingId PreQualificationVettingId,
    decimal EmrScore,
    decimal TrirScore,
    UtcTimestamp ExpiresAtUtc,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPreQualificationVettingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PreQualificationVettingApproved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the contractor is rejected.</summary>
public sealed record PreQualificationVettingRejectedEvent(
    PreQualificationVettingEventId EventId,
    PreQualificationVettingId PreQualificationVettingId,
    string Reason,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPreQualificationVettingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PreQualificationVettingRejected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the contractor's approval expires and re-vetting is required.</summary>
public sealed record PreQualificationVettingExpiredEvent(
    PreQualificationVettingEventId EventId,
    PreQualificationVettingId PreQualificationVettingId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPreQualificationVettingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PreQualificationVettingExpired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
