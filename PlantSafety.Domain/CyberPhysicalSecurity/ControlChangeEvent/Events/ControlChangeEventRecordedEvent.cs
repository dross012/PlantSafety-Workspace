// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.ControlChangeEvent
//  Layer:          Domain
//  File:           ControlChangeEventRecordedEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a control-system change is recorded (ingested from SCADA/HMI/ICS
//                  telemetry via Infrastructure.Integrations).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Events;

/// <summary>Raised when a new <see cref="Aggregates.ControlChangeEvent"/> is recorded.</summary>
public sealed record ControlChangeEventRecordedEvent(
    ControlChangeDomainEventId EventId,
    ControlChangeEventId       ControlChangeEventId,
    OtAssetId                  OtAssetId,
    UtcTimestamp                RecordedAtUtc,
    PerformedByActor            Actor,
    CorrelationId                CorrelationId,
    CausationId                  CausationId
) : IControlChangeEventDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ControlChangeEventRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RecordedAtUtc;
}
