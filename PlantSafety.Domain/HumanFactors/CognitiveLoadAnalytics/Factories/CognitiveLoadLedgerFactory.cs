// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveLoadLedgerFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing CognitiveLoadLedger aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Factories;

/// <summary>Default factory implementation for constructing <see cref="Aggregates.CognitiveLoadLedger"/> aggregates.</summary>
public sealed class CognitiveLoadLedgerFactory : ICognitiveLoadLedgerFactory
{
    /// <inheritdoc/>
    public Aggregates.CognitiveLoadLedger Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        ShiftType shiftType,
        UtcTimestamp shiftDateUtc)
        => Aggregates.CognitiveLoadLedger.Create(
            createdBy, createdAtUtc, correlationId, causationId, actor, tenantId, siteId, shiftType, shiftDateUtc);
}
