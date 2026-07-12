// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           PredictiveAnalyticsFactory.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing PredictiveAnalytics aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.Factories;

/// <summary>Default factory implementation for constructing <see cref="PredictiveAnalytics"/> aggregates.</summary>
public sealed class PredictiveAnalyticsFactory : IPredictiveAnalyticsFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.Analytics.PredictiveAnalytics.Aggregates.PredictiveAnalytics.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
