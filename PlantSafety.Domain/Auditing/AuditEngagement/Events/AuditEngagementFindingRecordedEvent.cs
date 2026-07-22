// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement
//  Layer:          Domain
//  File:           AuditEngagementFindingRecordedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised during Audit Workflow Step 3 (Findings) each time an AuditFinding is
//                  associated with this AuditEngagement, incrementing the running findings count.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditEngagement.Events;

/// <summary>Raised when a finding is recorded against an <see cref="AuditEngagement"/>.</summary>
public sealed record AuditEngagementFindingRecordedEvent(
    AuditEngagementEventId EventId,
    AuditEngagementId AuditEngagementId,
    Guid AuditFindingId,
    string Severity,
    int RunningFindingsCount,
    UtcTimestamp RecordedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAuditEngagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AuditEngagementFindingRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RecordedAtUtc;
}
