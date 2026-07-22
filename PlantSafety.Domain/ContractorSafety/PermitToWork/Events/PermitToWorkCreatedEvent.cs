// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PermitToWork
//  Layer:          Domain
//  File:           PermitToWorkCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real PTW lifecycle: drafting, hazard assessment,
//                  issuance, suspension, closure, and expiry.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PermitToWork.Events;

/// <summary>Raised when a new permit-to-work request is drafted.</summary>
public sealed record PermitToWorkDraftedEvent(
    PermitToWorkEventId EventId,
    PermitToWorkId PermitToWorkId,
    PermitType PermitType,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPermitToWorkDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitToWorkDrafted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the hazard assessment (and atmospheric testing, if applicable) is recorded.</summary>
public sealed record PermitToWorkHazardAssessmentRecordedEvent(
    PermitToWorkEventId EventId,
    PermitToWorkId PermitToWorkId,
    int HazardCount,
    decimal? AtmosphericTestScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPermitToWorkDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitToWorkHazardAssessmentRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the permit is issued and work is authorized to begin.</summary>
public sealed record PermitToWorkIssuedEvent(
    PermitToWorkEventId EventId,
    PermitToWorkId PermitToWorkId,
    UserId AuthorizedByUserId,
    UtcTimestamp ExpiresAtUtc,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPermitToWorkDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitToWorkIssued";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when an active permit is suspended due to an unsafe condition.</summary>
public sealed record PermitToWorkSuspendedEvent(
    PermitToWorkEventId EventId,
    PermitToWorkId PermitToWorkId,
    string Reason,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPermitToWorkDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitToWorkSuspended";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the permit is closed out.</summary>
public sealed record PermitToWorkClosedEvent(
    PermitToWorkEventId EventId,
    PermitToWorkId PermitToWorkId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPermitToWorkDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitToWorkClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the permit's validity window elapses without closure.</summary>
public sealed record PermitToWorkExpiredEvent(
    PermitToWorkEventId EventId,
    PermitToWorkId PermitToWorkId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IPermitToWorkDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PermitToWorkExpired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
