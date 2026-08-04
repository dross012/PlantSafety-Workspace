// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           AdvancedMetricsInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when an AdvancedMetricsLedger lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Exceptions;

/// <summary>Thrown when an AdvancedMetricsLedger lifecycle transition is not permitted.</summary>
public sealed class AdvancedMetricsInvalidStatusTransitionException : AdvancedMetricsDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public AdvancedMetricsInvalidStatusTransitionException(
        AdvancedMetricsLedgerId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'AdvancedMetricsLedger' {id} from '{from}' to '{to}'.",
            "AdvancedMetrics.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
