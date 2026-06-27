// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           PredictiveAnalyticsInvalidStatusTransitionException.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.Exceptions;

/// <summary>Thrown when a <see cref="PredictiveAnalytics"/> lifecycle transition is not permitted.</summary>
public sealed class PredictiveAnalyticsInvalidStatusTransitionException : PredictiveAnalyticsDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public PredictiveAnalyticsInvalidStatusTransitionException(
        PredictiveAnalyticsId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'PredictiveAnalytics' {id} from '{from}' to '{to}'.",
            "PredictiveAnalytics.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
