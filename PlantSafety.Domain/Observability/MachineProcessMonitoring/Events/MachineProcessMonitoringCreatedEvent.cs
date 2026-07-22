// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.MachineProcessMonitoring
//  Layer:          Domain
//  File:           MachineProcessMonitoringCreatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new MachineProcessMonitoring aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.MachineProcessMonitoring.Events;

/// <summary>Raised when a new <see cref="MachineProcessMonitoring"/> aggregate is created.</summary>
public sealed record MachineProcessMonitoringCreatedEvent(
    MachineProcessMonitoringEventId EventId,
    MachineProcessMonitoringId      MachineProcessMonitoringId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IMachineProcessMonitoringDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "MachineProcessMonitoringCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}