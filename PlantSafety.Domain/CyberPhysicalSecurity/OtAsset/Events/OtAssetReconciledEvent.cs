// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity.OtAsset
//  Layer:          Domain
//  File:           OtAssetReconciledEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when an OtAsset is reconciled against the periodic OT asset inventory
//                  reconciliation scheduling hook (see project_plantsafety_domains.md
//                  CyberPhysicalSecurity scheduling hooks).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.ValueObjects.Ids;

namespace PlantSafety.Domain.CyberPhysicalSecurity.OtAsset.Events;

/// <summary>Raised when an <see cref="Aggregates.OtAsset"/> is reconciled during a periodic inventory sweep.</summary>
public sealed record OtAssetReconciledEvent(
    OtAssetEventId EventId,
    OtAssetId       OtAssetId,
    UtcTimestamp    ReconciledAtUtc,
    PerformedByActor Actor,
    CorrelationId   CorrelationId,
    CausationId     CausationId
) : IOtAssetDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "OtAssetReconciled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ReconciledAtUtc;
}
