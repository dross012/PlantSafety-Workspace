// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Application
//  File:           PredictiveAnalyticsStatusResult.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Shared result payload for the PredictiveAnalytics lifecycle-transition
//                  commands (Review, Action, Close, Archive). Mirrors
//                  PermitsStatusResult's identical precedent.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Analytics.PredictiveAnalytics.Commands;

/// <summary>Result payload shared by PredictiveAnalytics lifecycle-transition commands.</summary>
/// <param name="PredictiveAnalyticsId">Identity of the affected PredictiveAnalytics aggregate.</param>
/// <param name="Status">Lifecycle status after the transition.</param>
public sealed record PredictiveAnalyticsStatusResult(Guid PredictiveAnalyticsId, string Status);
