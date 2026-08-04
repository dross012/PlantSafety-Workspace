// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           IEsgDisclosureReportFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing EsgDisclosureReport aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Interfaces;

/// <summary>Factory interface for constructing <see cref="Aggregates.EsgDisclosureReport"/> aggregates.</summary>
public interface IEsgDisclosureReportFactory
{
    /// <summary>Generates a new version-1 draft EsgDisclosureReport with the supplied parameters.</summary>
    Aggregates.EsgDisclosureReport Generate(
        UserId generatedBy,
        UtcTimestamp generatedAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        AdvancedMetricsLedgerId sourceLedgerId,
        EsgDisclosureFramework frameworks,
        UtcTimestamp reportingPeriodStartUtc,
        UtcTimestamp reportingPeriodEndUtc,
        string methodologyVersion);
}
