// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           AdvancedMetricsNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an AdvancedMetricsLedger aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Exceptions;

/// <summary>Thrown when an AdvancedMetricsLedger aggregate lookup finds no match.</summary>
public sealed class AdvancedMetricsNotFoundException : AdvancedMetricsDomainException
{
    /// <summary>Creates a not-found exception for the given AdvancedMetricsLedger identity.</summary>
    public AdvancedMetricsNotFoundException(AdvancedMetricsLedgerId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"AdvancedMetricsLedger with id '{id}' was not found.",
            "AdvancedMetrics.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
