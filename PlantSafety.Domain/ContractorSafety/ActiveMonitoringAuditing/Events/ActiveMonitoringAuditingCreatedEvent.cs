// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.ActiveMonitoringAuditing
//  Layer:          Domain
//  File:           ActiveMonitoringAuditingCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real monitoring/audit lifecycle: scheduling,
//                  observation recording, unsafe-condition flagging, follow-up, and resolution.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Events;

/// <summary>Raised when a new monitoring/audit visit is scheduled.</summary>
public sealed record ActiveMonitoringAuditingScheduledEvent(
    ActiveMonitoringAuditingEventId EventId,
    ActiveMonitoringAuditingId ActiveMonitoringAuditingId,
    ContractorMonitoringCategory Category,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IActiveMonitoringAuditingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ActiveMonitoringAuditingScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when an observation is recorded with no unsafe conditions found.</summary>
public sealed record ActiveMonitoringAuditingObservationRecordedEvent(
    ActiveMonitoringAuditingEventId EventId,
    ActiveMonitoringAuditingId ActiveMonitoringAuditingId,
    decimal ComplianceScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IActiveMonitoringAuditingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ActiveMonitoringAuditingObservationRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when an unsafe condition or behavior is flagged during the observation.</summary>
public sealed record ActiveMonitoringAuditingUnsafeConditionFlaggedEvent(
    ActiveMonitoringAuditingEventId EventId,
    ActiveMonitoringAuditingId ActiveMonitoringAuditingId,
    int UnsafeConditionCount,
    string FindingNotes,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IActiveMonitoringAuditingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ActiveMonitoringAuditingUnsafeConditionFlagged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a follow-up visit is scheduled to verify correction.</summary>
public sealed record ActiveMonitoringAuditingFollowUpScheduledEvent(
    ActiveMonitoringAuditingEventId EventId,
    ActiveMonitoringAuditingId ActiveMonitoringAuditingId,
    UtcTimestamp DueAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IActiveMonitoringAuditingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ActiveMonitoringAuditingFollowUpScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DueAtUtc;
}

/// <summary>Raised when the flagged condition is resolved.</summary>
public sealed record ActiveMonitoringAuditingResolvedEvent(
    ActiveMonitoringAuditingEventId EventId,
    ActiveMonitoringAuditingId ActiveMonitoringAuditingId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IActiveMonitoringAuditingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ActiveMonitoringAuditingResolved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
