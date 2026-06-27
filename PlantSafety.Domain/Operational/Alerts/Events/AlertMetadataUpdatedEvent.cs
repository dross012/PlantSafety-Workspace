// ============================================================================
//  File:           AlertMetadataUpdatedEvent.cs
//  Created:        2026-05-26
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Events;

public sealed record AlertMetadataUpdatedEvent(
    AlertEventId EventId,
    AlertId AlertId,
    AlertMetadataKey Key,
    AlertMetadataValue NewValue,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAlertDomainEvent
{
    public int EventVersion => 1;
    public string SchemaVersion => "2026.1";

    public string EventType => "AlertMetadataUpdated";
}
