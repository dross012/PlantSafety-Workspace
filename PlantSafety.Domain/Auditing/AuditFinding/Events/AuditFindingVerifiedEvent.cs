// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditFinding
//  Layer:          Domain
//  File:           AuditFindingVerifiedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised during Audit Workflow Step 4 (Corrective Actions) when the
//                  effectiveness of a completed corrective action is verified and the
//                  AuditFinding is closed out.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditFinding.Events;

/// <summary>Raised when a completed corrective action is verified as effective and the <see cref="AuditFinding"/> is closed.</summary>
public sealed record AuditFindingVerifiedEvent(
    AuditFindingEventId EventId,
    AuditFindingId AuditFindingId,
    UtcTimestamp VerifiedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAuditFindingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AuditFindingVerified";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => VerifiedAtUtc;
}
