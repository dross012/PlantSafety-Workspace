// ============================================================================
//  File:           AlertArchivedEvent.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Events;

public sealed record AlertArchivedEvent(
    AlertEventId EventId,
    AlertId AlertId,
    UtcTimestamp ArchivedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAlertDomainEvent
{
    public string EventType => "AlertArchived";
    public int EventVersion => 1;
    public string SchemaVersion => "2026.1";
    public UtcTimestamp OccurredAtUtc => ArchivedAtUtc;
}
