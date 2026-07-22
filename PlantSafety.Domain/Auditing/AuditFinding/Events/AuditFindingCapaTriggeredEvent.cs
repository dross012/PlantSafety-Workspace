// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditFinding
//  Layer:          Domain
//  File:           AuditFindingCapaTriggeredEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Raised during Audit Workflow Step 4 (Corrective Actions) when a Corrective
//                  and Preventive Action (CAPA) is triggered for an AuditFinding: assigned to a
//                  responsible user with a due date. Per PlantSafety Program Structure Appendix
//                  G, corrective action categories include Engineering Controls, Administrative
//                  Controls, PPE Controls, Training, Maintenance, Procedural Updates, and
//                  Environmental Controls.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditFinding.Events;

/// <summary>Raised when a corrective/preventive action (CAPA) is triggered for an <see cref="AuditFinding"/>.</summary>
public sealed record AuditFindingCapaTriggeredEvent(
    AuditFindingEventId EventId,
    AuditFindingId AuditFindingId,
    UserId AssignedToUserId,
    UtcTimestamp DueDateUtc,
    string Severity,
    UtcTimestamp TriggeredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAuditFindingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AuditFindingCapaTriggered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => TriggeredAtUtc;
}
