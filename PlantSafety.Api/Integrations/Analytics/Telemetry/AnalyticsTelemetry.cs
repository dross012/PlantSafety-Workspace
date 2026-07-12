// ============================================================================
// File: AnalyticsTelemetry.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Telemetry span name and OpenTelemetry attribute key constants for the Integrations.Analytics module.
// ============================================================================
//
// Onboarding Notes:
// - Referenced by OpenTelemetry instrumentation. Infrastructure registers ActivitySource instances; this file only declares the name constants.
//
// Rationale:
// - Centralising tag keys prevents typos in distributed trace attributes and ensures consistent filtering in Azure Monitor and Jaeger.
//
// Future Extension Seams:
// - Add custom span attribute keys (TenantId, SiteId, RecordCount) as Integrations.Analytics telemetry requirements expand.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Analytics.Telemetry;

/// <summary>Telemetry span names and OpenTelemetry attribute key constants for Integrations.Analytics.</summary>
public static class AnalyticsTelemetry
{
    /// <summary>Span name for a Integrations.Analytics read query.</summary>
    public const string QuerySpan = "Integrations.Analytics.query";

    /// <summary>Span name for a Integrations.Analytics write command.</summary>
    public const string CommandSpan = "Integrations.Analytics.command";

    /// <summary>Span attribute key for the tenant identifier.</summary>
    public const string TenantIdTag = "plantsafety.tenant_id";

    /// <summary>Span attribute key for the site identifier.</summary>
    public const string SiteIdTag = "plantsafety.site_id";

    /// <summary>Span attribute key for the number of records returned.</summary>
    public const string RecordCountTag = "plantsafety.integrations.analytics.record_count";

    /// <summary>Span attribute key for whether the result was served from cache.</summary>
    public const string CacheHitTag = "plantsafety.integrations.analytics.cache_hit";
}
