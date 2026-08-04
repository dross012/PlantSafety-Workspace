// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.ControlChangeEvent
//  Layer:          Domain
//  File:           UnauthorizedControlChangeDetectedEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a control-system change is flagged as unauthorized. Drives the
//                  "mean time to detect unauthorized control change" KPI and feeds
//                  SabotageIndicator correlation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;
using PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.ControlChangeEvent.Events;

/// <summary>Raised when a <see cref="Aggregates.ControlChangeEvent"/> is flagged as unauthorized.</summary>
public sealed record UnauthorizedControlChangeDetectedEvent(
    ControlChangeDomainEventId EventId,
    ControlChangeEventId       ControlChangeEventId,
    OtAssetId                  OtAssetId,
    string                     ChangedParameter,
    UtcTimestamp                DetectedAtUtc,
    PerformedByActor            Actor,
    CorrelationId                CorrelationId,
    CausationId                  CausationId
) : IControlChangeEventDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "UnauthorizedControlChangeDetected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => DetectedAtUtc;
}
