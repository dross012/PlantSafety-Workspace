// ============================================================================
// File: WellnessTelemetry.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-20
// Last Updated: 2026-08-20
// Description: Telemetry span name and OpenTelemetry attribute key constants for the Wellness module.
// ============================================================================
//
// Onboarding Notes:
// - Referenced by OpenTelemetry instrumentation. Infrastructure registers ActivitySource instances; this file only declares the name constants.
//
// Rationale:
// - Centralising tag keys prevents typos in distributed trace attributes and ensures consistent filtering in Azure Monitor and Jaeger.
//
// Future Extension Seams:
// - Add custom span attribute keys (TenantId, SiteId, RecordCount) as Wellness telemetry requirements expand.
//
// ============================================================================

namespace PlantSafety.Api.Operational.Wellness.Telemetry;

/// <summary>Telemetry span names and OpenTelemetry attribute key constants for Wellness.</summary>
public static class WellnessTelemetry
{
    /// <summary>Span name for a Wellness read query.</summary>
    public const string QuerySpan = "Operational.WellnessLC_.WellnessLC_.query";

    /// <summary>Span name for a Wellness write command.</summary>
    public const string CommandSpan = "Operational.WellnessLC_.WellnessLC_.command";

    /// <summary>Span attribute key for the tenant identifier.</summary>
    public const string TenantIdTag = "plantsafety.tenant_id";

    /// <summary>Span attribute key for the site identifier.</summary>
    public const string SiteIdTag = "plantsafety.site_id";

    /// <summary>Span attribute key for the number of records returned.</summary>
    public const string RecordCountTag = "plantsafety.Operational.WellnessLC_.record_count";

    /// <summary>Span attribute key for whether the result was served from cache.</summary>
    public const string CacheHitTag = "plantsafety.Operational.WellnessLC_.cache_hit";
}