// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.OtAsset
//  Layer:          Domain
//  File:           OtAssetRegisteredEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new OtAsset is registered in the OT Asset Inventory.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Events;

/// <summary>Raised when a new <see cref="Aggregates.OtAsset"/> aggregate is registered.</summary>
public sealed record OtAssetRegisteredEvent(
    OtAssetEventId EventId,
    OtAssetId       OtAssetId,
    UtcTimestamp    RegisteredAtUtc,
    PerformedByActor Actor,
    CorrelationId   CorrelationId,
    CausationId     CausationId
) : IOtAssetDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "OtAssetRegistered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => RegisteredAtUtc;
}
