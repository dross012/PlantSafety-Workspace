// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           IPredictiveAnalyticsFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing PredictiveAnalytics aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.Interfaces;

/// <summary>Factory interface for constructing <see cref="PredictiveAnalytics"/> aggregates.</summary>
public interface IPredictiveAnalyticsFactory
{
    /// <summary>Creates a new PredictiveAnalytics aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
