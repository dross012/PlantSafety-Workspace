// ============================================================================
//  File:           AlertInProgressEvent.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Events;

public sealed record AlertInProgressEvent(
    AlertEventId EventId,
    AlertId AlertId,
    UtcTimestamp InProgressAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAlertDomainEvent
{
    public string EventType => "AlertInProgress";
    public int EventVersion => 1;
    public string SchemaVersion => "2026.1";
    public UtcTimestamp OccurredAtUtc => InProgressAtUtc;
}
