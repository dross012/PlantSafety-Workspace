// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           PredictiveAnalyticsNotFoundException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.Exceptions;

/// <summary>Thrown when a <see cref="PredictiveAnalytics"/> aggregate lookup finds no match.</summary>
public sealed class PredictiveAnalyticsNotFoundException : PredictiveAnalyticsDomainException
{
    /// <summary>Creates a not-found exception for the given PredictiveAnalytics identity.</summary>
    public PredictiveAnalyticsNotFoundException(PredictiveAnalyticsId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "PredictiveAnalytics with id '{id}' was not found.",
            "PredictiveAnalytics.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
