// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Domain
//  File:           ProductionRunEventRecordedEvent.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Raised whenever a timeline event (Step/Pause/Error/Resume/Downtime/etc.) is
//                  recorded against an in-progress ProductionRun. Backs
//                  GetRunTimelineQuery/GetRunEventsQuery/GetEventHistoryQuery/GetProductionTimelineQuery.
//                  When IsDowntime is true, the Infrastructure repository additionally persists a
//                  row into [Operational].[ProductionDowntimeEvents], feeding the real
//                  Observability.Telemetry.ValueObjects.DowntimeEvent-shaped bottleneck-detection
//                  read path (see GetBottlenecksAsync's handler). Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.ProductionTracking.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.ProductionTracking.Events;

/// <summary>Raised whenever a timeline event is recorded against a ProductionRun.</summary>
public sealed record ProductionRunEventRecordedEvent(
    Guid EventId,
    ProductionRunId ProductionRunId,
    string RunEventType,
    string Description,
    bool IsDowntime,
    string? DowntimeReasonCode,
    double DowntimeMinutes,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId) : IProductionTrackingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProductionRunEventRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
