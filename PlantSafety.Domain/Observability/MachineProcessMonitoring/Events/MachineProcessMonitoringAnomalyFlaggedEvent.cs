// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.MachineProcessMonitoring
//  Layer:          Domain
//  File:           MachineProcessMonitoringAnomalyFlaggedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when an AI/ML process anomaly is flagged (e.g., unexpected machine
//                  startup, bypassed guard, disabled interlock).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.MachineProcessMonitoring.Events;

/// <summary>Raised when a process anomaly is flagged for a <see cref="MachineProcessMonitoring"/> channel.</summary>
public sealed record MachineProcessMonitoringAnomalyFlaggedEvent(
    MachineProcessMonitoringEventId EventId,
    MachineProcessMonitoringId      MachineProcessMonitoringId,
    string                          AnomalyDescription,
    UtcTimestamp                    FlaggedAtUtc,
    PerformedByActor                Actor,
    CorrelationId                   CorrelationId,
    CausationId                     CausationId
) : IMachineProcessMonitoringDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineProcessMonitoringAnomalyFlagged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => FlaggedAtUtc;
}
