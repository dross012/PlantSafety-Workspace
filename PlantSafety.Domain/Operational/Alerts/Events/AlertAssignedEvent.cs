// ============================================================================
//  File:           AlertAssignedEvent.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Events;

public sealed record AlertAssignedEvent(
    AlertEventId EventId,
    AlertId AlertId,
    string AssignedTo,
    UtcTimestamp AssignedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAlertDomainEvent
{
    public string EventType => "AlertAssigned";
    public int EventVersion => 1;
    public string SchemaVersion => "2026.1";
    public UtcTimestamp OccurredAtUtc => AssignedAtUtc;
}
