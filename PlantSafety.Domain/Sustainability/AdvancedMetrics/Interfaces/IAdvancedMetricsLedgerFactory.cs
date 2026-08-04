// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           IAdvancedMetricsLedgerFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing AdvancedMetricsLedger aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Interfaces;

/// <summary>Factory interface for constructing <see cref="Aggregates.AdvancedMetricsLedger"/> aggregates.</summary>
public interface IAdvancedMetricsLedgerFactory
{
    /// <summary>Opens a new AdvancedMetricsLedger with the supplied parameters.</summary>
    Aggregates.AdvancedMetricsLedger Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        UtcTimestamp reportingPeriodStartUtc,
        UtcTimestamp reportingPeriodEndUtc);
}
