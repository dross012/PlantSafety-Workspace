// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.FacilityTrafficMonitoring
//  Layer:          Domain
//  File:           FacilityTrafficMonitoringCreatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new FacilityTrafficMonitoring aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Events;

/// <summary>Raised when a new <see cref="FacilityTrafficMonitoring"/> aggregate is created.</summary>
public sealed record FacilityTrafficMonitoringCreatedEvent(
    FacilityTrafficMonitoringEventId EventId,
    FacilityTrafficMonitoringId      FacilityTrafficMonitoringId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IFacilityTrafficMonitoringDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "FacilityTrafficMonitoringCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}