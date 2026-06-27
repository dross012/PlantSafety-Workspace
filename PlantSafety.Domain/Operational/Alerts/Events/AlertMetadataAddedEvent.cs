// ============================================================================
//  File:           AlertMetadataAddedEvent.cs
//  Created:        2026-05-26
// ============================================================================

namespace PlantSafety.Domain.Operational.Alerts.Events;

public sealed record AlertMetadataAddedEvent(
    AlertEventId EventId,
    AlertId AlertId,
    AlertMetadataKey Key,
    AlertMetadataValue Value,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAlertDomainEvent
{
    public string EventType => "AlertMetadataAdded";
    public int EventVersion => 1;
    public string SchemaVersion => "2026.1";

}
