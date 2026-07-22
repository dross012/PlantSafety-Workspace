// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.PPEBehavioralMonitoring
//  Layer:          Domain
//  File:           PPEBehavioralMonitoringCreatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new PPEBehavioralMonitoring aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Events;

/// <summary>Raised when a new <see cref="PPEBehavioralMonitoring"/> aggregate is created.</summary>
public sealed record PPEBehavioralMonitoringCreatedEvent(
    PPEBehavioralMonitoringEventId EventId,
    PPEBehavioralMonitoringId      PPEBehavioralMonitoringId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IPPEBehavioralMonitoringDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PPEBehavioralMonitoringCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}