// ============================================================================
// File: EngagementDiagnostics.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: ActivitySource names and metric name constants for the Engagement Analytics module.
// ============================================================================
//
// Onboarding Notes:
// - Activity source names are consumed by OpenTelemetry instrumentation in the Infrastructure layer. Presentation declares names; Infrastructure registers ActivitySource instances.
//
// Rationale:
// - Separating diagnostic constants from controllers prevents magic strings and ensures consistent trace names across all layers.
//
// Future Extension Seams:
// - Add custom metric names (e.g. plantsafety.analytics.engagement.cache_hit_ratio) when fine-grained OTel metrics are instrumented.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Engagement.Diagnostics;

/// <summary>Diagnostic constants for the Engagement Analytics module.</summary>
public static class EngagementDiagnostics
{
    /// <summary>OpenTelemetry ActivitySource name for Engagement traces.</summary>
    public const string ActivitySourceName = "PlantSafety.Analytics.Engagement";

    /// <summary>Histogram metric name for Engagement query duration in milliseconds.</summary>
    public const string QueryDurationMetric = "plantsafety.analytics.engagement.query_duration_ms";

    /// <summary>Counter metric name for Engagement cache hit rate.</summary>
    public const string CacheHitMetric = "plantsafety.analytics.engagement.cache_hit_ratio";
}