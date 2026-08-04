// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SensorIntegrityCheck
//  Layer:          Domain
//  File:           SensorIntegrityCheckFailedEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised specifically when a sensor-integrity check fails, indicating possible
//                  sensor tampering. Feeds SabotageIndicator correlation.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Events;

/// <summary>Raised when a <see cref="Aggregates.SensorIntegrityCheck"/> fails.</summary>
public sealed record SensorIntegrityCheckFailedEvent(
    SensorIntegrityCheckEventId EventId,
    SensorIntegrityCheckId       SensorIntegrityCheckId,
    OtAssetId                    OtAssetId,
    string                       Details,
    UtcTimestamp                  FailedAtUtc,
    PerformedByActor              Actor,
    CorrelationId                  CorrelationId,
    CausationId                    CausationId
) : ISensorIntegrityCheckDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SensorIntegrityCheckFailed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => FailedAtUtc;
}
