// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           EsgDisclosureReportNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an EsgDisclosureReport aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Exceptions;

/// <summary>Thrown when an EsgDisclosureReport aggregate lookup finds no match.</summary>
public sealed class EsgDisclosureReportNotFoundException : AdvancedMetricsDomainException
{
    /// <summary>Creates a not-found exception for the given EsgDisclosureReport identity.</summary>
    public EsgDisclosureReportNotFoundException(EsgDisclosureReportId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"EsgDisclosureReport with id '{id}' was not found.",
            "AdvancedMetrics.EsgDisclosureReport.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
