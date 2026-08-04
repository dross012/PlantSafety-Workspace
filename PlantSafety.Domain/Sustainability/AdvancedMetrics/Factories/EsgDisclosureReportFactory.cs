// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           EsgDisclosureReportFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing EsgDisclosureReport aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Factories;

/// <summary>Default factory implementation for constructing <see cref="Aggregates.EsgDisclosureReport"/> aggregates.</summary>
public sealed class EsgDisclosureReportFactory : IEsgDisclosureReportFactory
{
    /// <inheritdoc/>
    public Aggregates.EsgDisclosureReport Generate(
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
        string methodologyVersion)
        => Aggregates.EsgDisclosureReport.Generate(
            generatedBy, generatedAtUtc, correlationId, causationId, actor,
            tenantId, siteId, sourceLedgerId, frameworks, reportingPeriodStartUtc, reportingPeriodEndUtc, methodologyVersion);
}
