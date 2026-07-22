// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement
//  Layer:          Domain
//  File:           AuditEngagementStartedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised during Audit Workflow Step 2 (Execution) when field work begins:
//                  inspections performed, documents reviewed, personnel interviewed, evidence collected.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditEngagement.Events;

/// <summary>Raised when execution of an <see cref="AuditEngagement"/> begins.</summary>
public sealed record AuditEngagementStartedEvent(
    AuditEngagementEventId EventId,
    AuditEngagementId AuditEngagementId,
    UtcTimestamp StartedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAuditEngagementDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AuditEngagementStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => StartedAtUtc;
}
