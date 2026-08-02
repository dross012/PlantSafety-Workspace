// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Application
//  File:           CreatePredictiveAnalyticsResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Result payload for CreatePredictiveAnalyticsCommand.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Analytics.PredictiveAnalytics.Commands;

/// <summary>Result of a successful <see cref="CreatePredictiveAnalyticsCommand"/> execution.</summary>
/// <param name="PredictiveAnalyticsId">Identity of the newly created PredictiveAnalytics aggregate.</param>
/// <param name="Status">Lifecycle status of the newly created record.</param>
public sealed record CreatePredictiveAnalyticsResult(
    Guid PredictiveAnalyticsId,
    string Status);
