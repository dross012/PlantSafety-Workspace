// ============================================================================
//  File:           AlertCreatedEvent.cs
//  Created:        2026-05-26
//  SchemaVersion:  2026.1
//  Description:
//      Emitted when a new Alert is created.
//      Payload includes full initial state.
// ============================================================================
namespace PlantSafety.Domain.Operational.Alerts.Events;

public sealed record AlertCreatedEvent(
    AlertEventId EventId,
    AlertId AlertId,
    AlertType AlertType,
    AlertSeverity Severity,
    string Title,
    string Description,
    string SourceModule,
    UtcTimestamp CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAlertDomainEvent
{
    public string EventType => "AlertCreated";
    public int EventVersion => 1;
    public string SchemaVersion => "2026.1";
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
