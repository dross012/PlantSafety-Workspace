// ============================================================================
// File: BehaviorTelemetry.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Telemetry span name and OpenTelemetry attribute key constants for the Behavior module.
// ============================================================================
//
// Onboarding Notes:
// - Referenced by OpenTelemetry instrumentation. Infrastructure registers ActivitySource instances; this file only declares the name constants.
//
// Rationale:
// - Centralising tag keys prevents typos in distributed trace attributes and ensures consistent filtering in Azure Monitor and Jaeger.
//
// Future Extension Seams:
// - Add custom span attribute keys (TenantId, SiteId, RecordCount) as Behavior telemetry requirements expand.
//
// ============================================================================

namespace PlantSafety.Api.Analytics.Culture.Behavior.Telemetry;

/// <summary>Telemetry span names and OpenTelemetry attribute key constants for Behavior.</summary>
public static class BehaviorTelemetry
{
    /// <summary>Span name for a Behavior read query.</summary>
    public const string QuerySpan = "Analytics.Culture.BehaviorLC_.BehaviorLC_.query";

    /// <summary>Span name for a Behavior write command.</summary>
    public const string CommandSpan = "Analytics.Culture.BehaviorLC_.BehaviorLC_.command";

    /// <summary>Span attribute key for the tenant identifier.</summary>
    public const string TenantIdTag = "plantsafety.tenant_id";

    /// <summary>Span attribute key for the site identifier.</summary>
    public const string SiteIdTag = "plantsafety.site_id";

    /// <summary>Span attribute key for the number of records returned.</summary>
    public const string RecordCountTag = "plantsafety.Analytics.Culture.BehaviorLC_.record_count";

    /// <summary>Span attribute key for whether the result was served from cache.</summary>
    public const string CacheHitTag = "plantsafety.Analytics.Culture.BehaviorLC_.cache_hit";
}