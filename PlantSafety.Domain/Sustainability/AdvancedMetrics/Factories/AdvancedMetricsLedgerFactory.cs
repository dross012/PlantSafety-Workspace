// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           AdvancedMetricsLedgerFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing AdvancedMetricsLedger aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Factories;

/// <summary>Default factory implementation for constructing <see cref="Aggregates.AdvancedMetricsLedger"/> aggregates.</summary>
public sealed class AdvancedMetricsLedgerFactory : IAdvancedMetricsLedgerFactory
{
    /// <inheritdoc/>
    public Aggregates.AdvancedMetricsLedger Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        UtcTimestamp reportingPeriodStartUtc,
        UtcTimestamp reportingPeriodEndUtc)
        => Aggregates.AdvancedMetricsLedger.Create(
            createdBy, createdAtUtc, correlationId, causationId, actor,
            tenantId, siteId, reportingPeriodStartUtc, reportingPeriodEndUtc);
}
