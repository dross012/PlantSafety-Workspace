// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           EsgDisclosureReportImmutableException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a mutation is attempted against a Published (immutable)
//                  EsgDisclosureReport. Corrections must go through Supersede -> a new versioned
//                  report, per the module's data-governance rule.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Exceptions;

/// <summary>Thrown when a mutation is attempted against a Published (immutable) EsgDisclosureReport.</summary>
public sealed class EsgDisclosureReportImmutableException : AdvancedMetricsDomainException
{
    /// <summary>Creates an immutability-violation exception for the given EsgDisclosureReport identity.</summary>
    public EsgDisclosureReportImmutableException(EsgDisclosureReportId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"EsgDisclosureReport {id} is published and immutable. Corrections must be made via a superseding versioned report.",
            "AdvancedMetrics.EsgDisclosureReport.Immutable",
            "Validation",
            "High",
            correlationId,
            causationId)
    {
    }
}
