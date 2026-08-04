// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           ICognitiveLoadLedgerFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing CognitiveLoadLedger aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Interfaces;

/// <summary>Factory interface for constructing <see cref="Aggregates.CognitiveLoadLedger"/> aggregates.</summary>
public interface ICognitiveLoadLedgerFactory
{
    /// <summary>Opens a new CognitiveLoadLedger with the supplied parameters.</summary>
    Aggregates.CognitiveLoadLedger Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        ShiftType shiftType,
        UtcTimestamp shiftDateUtc);
}
