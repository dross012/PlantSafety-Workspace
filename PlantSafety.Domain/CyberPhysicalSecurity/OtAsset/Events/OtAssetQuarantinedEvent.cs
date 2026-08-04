// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.OtAsset
//  Layer:          Domain
//  File:           OtAssetQuarantinedEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when an OtAsset is isolated from the OT network pending investigation,
//                  typically in response to a SabotageIndicator or CyberPhysicalIncident.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Events;

/// <summary>Raised when an <see cref="Aggregates.OtAsset"/> is quarantined.</summary>
public sealed record OtAssetQuarantinedEvent(
    OtAssetEventId EventId,
    OtAssetId       OtAssetId,
    string          Reason,
    UtcTimestamp    QuarantinedAtUtc,
    PerformedByActor Actor,
    CorrelationId   CorrelationId,
    CausationId     CausationId
) : IOtAssetDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "OtAssetQuarantined";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => QuarantinedAtUtc;
}
