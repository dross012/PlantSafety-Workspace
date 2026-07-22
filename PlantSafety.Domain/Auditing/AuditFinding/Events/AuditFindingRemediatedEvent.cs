// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditFinding
//  Layer:          Domain
//  File:           AuditFindingRemediatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised when the assigned corrective action for an AuditFinding has been
//                  performed and is awaiting verification of effectiveness.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditFinding.Events;

/// <summary>Raised when an <see cref="AuditFinding"/> corrective action has been performed.</summary>
public sealed record AuditFindingRemediatedEvent(
    AuditFindingEventId EventId,
    AuditFindingId AuditFindingId,
    string RemediationNotes,
    UtcTimestamp RemediatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAuditFindingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AuditFindingRemediated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RemediatedAtUtc;
}
