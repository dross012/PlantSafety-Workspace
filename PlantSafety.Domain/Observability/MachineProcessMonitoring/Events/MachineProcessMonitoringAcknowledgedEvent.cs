// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.MachineProcessMonitoring
//  Layer:          Domain
//  File:           MachineProcessMonitoringAcknowledgedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when an operator/supervisor acknowledges a threshold breach or
//                  anomaly flag and begins investigating.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.MachineProcessMonitoring.Events;

/// <summary>Raised when a <see cref="MachineProcessMonitoring"/> breach/anomaly is acknowledged.</summary>
public sealed record MachineProcessMonitoringAcknowledgedEvent(
    MachineProcessMonitoringEventId EventId,
    MachineProcessMonitoringId      MachineProcessMonitoringId,
    UserId                          AcknowledgedBy,
    UtcTimestamp                    AcknowledgedAtUtc,
    PerformedByActor                Actor,
    CorrelationId                   CorrelationId,
    CausationId                     CausationId
) : IMachineProcessMonitoringDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineProcessMonitoringAcknowledged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => AcknowledgedAtUtc;
}
