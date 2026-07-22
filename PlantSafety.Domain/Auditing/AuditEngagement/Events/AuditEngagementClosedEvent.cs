// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement
//  Layer:          Domain
//  File:           AuditEngagementClosedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised during Audit Workflow Step 5 (Reporting) when an AuditEngagement is
//                  closed: final compliance score/rating computed, report issued to leadership.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditEngagement.Events;

/// <summary>Raised when an <see cref="AuditEngagement"/> is closed out.</summary>
public sealed record AuditEngagementClosedEvent(
    AuditEngagementEventId EventId,
    AuditEngagementId AuditEngagementId,
    decimal? ComplianceScore,
    string ComplianceRating,
    int FindingsCount,
    UtcTimestamp ClosedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAuditEngagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AuditEngagementClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ClosedAtUtc;
}
