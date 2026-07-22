// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement
//  Layer:          Domain
//  File:           AuditEngagementScheduledEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised during Audit Workflow Step 1 (Planning) when an AuditEngagement is
//                  scheduled: discipline confirmed, lead auditor assigned, execution date set.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditEngagement.Events;

/// <summary>Raised when an <see cref="AuditEngagement"/> is scheduled.</summary>
public sealed record AuditEngagementScheduledEvent(
    AuditEngagementEventId EventId,
    AuditEngagementId AuditEngagementId,
    UserId LeadAuditorId,
    UtcTimestamp ScheduledForUtc,
    UtcTimestamp PlannedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAuditEngagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AuditEngagementScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => PlannedAtUtc;
}
