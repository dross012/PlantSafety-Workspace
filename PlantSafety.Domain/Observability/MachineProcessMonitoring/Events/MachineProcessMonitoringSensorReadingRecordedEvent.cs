// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.MachineProcessMonitoring
//  Layer:          Domain
//  File:           MachineProcessMonitoringSensorReadingRecordedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new machine/process sensor reading is recorded, including
//                  whether it breached the configured safety threshold ("real-time hazard
//                  alerts").
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.MachineProcessMonitoring.Events;

/// <summary>Raised when a sensor reading is recorded for a <see cref="MachineProcessMonitoring"/> channel.</summary>
public sealed record MachineProcessMonitoringSensorReadingRecordedEvent(
    MachineProcessMonitoringEventId EventId,
    MachineProcessMonitoringId      MachineProcessMonitoringId,
    decimal                         Reading,
    bool                            ThresholdBreached,
    UtcTimestamp                    RecordedAtUtc,
    PerformedByActor                Actor,
    CorrelationId                   CorrelationId,
    CausationId                     CausationId
) : IMachineProcessMonitoringDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineProcessMonitoringSensorReadingRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RecordedAtUtc;
}
