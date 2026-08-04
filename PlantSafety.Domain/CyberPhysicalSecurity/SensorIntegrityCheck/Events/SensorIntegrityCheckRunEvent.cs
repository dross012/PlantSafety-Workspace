// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.SensorIntegrityCheck
//  Layer:          Domain
//  File:           SensorIntegrityCheckRunEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a sensor-integrity check completes (any result). Feeds the
//                  recurring sensor integrity check scheduling hook and the pass-rate KPI.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.SensorIntegrityCheck.Events;

/// <summary>Raised when a new <see cref="Aggregates.SensorIntegrityCheck"/> completes.</summary>
public sealed record SensorIntegrityCheckRunEvent(
    SensorIntegrityCheckEventId EventId,
    SensorIntegrityCheckId       SensorIntegrityCheckId,
    OtAssetId                    OtAssetId,
    SensorIntegrityCheckResult   Result,
    UtcTimestamp                  CheckedAtUtc,
    PerformedByActor              Actor,
    CorrelationId                  CorrelationId,
    CausationId                    CausationId
) : ISensorIntegrityCheckDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SensorIntegrityCheckRun";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CheckedAtUtc;
}
