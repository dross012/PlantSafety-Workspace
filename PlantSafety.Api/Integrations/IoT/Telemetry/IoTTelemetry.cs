// ============================================================================
// File: IoTTelemetry.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Telemetry span name and OpenTelemetry attribute key constants for the Integrations.IoT module.
// ============================================================================
//
// Onboarding Notes:
// - Referenced by OpenTelemetry instrumentation. Infrastructure registers ActivitySource instances; this file only declares the name constants.
//
// Rationale:
// - Centralising tag keys prevents typos in distributed trace attributes and ensures consistent filtering in Azure Monitor and Jaeger.
//
// Future Extension Seams:
// - Add custom span attribute keys (TenantId, SiteId, RecordCount) as Integrations.IoT telemetry requirements expand.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.IoT.Telemetry;

/// <summary>Telemetry span names and OpenTelemetry attribute key constants for Integrations.IoT.</summary>
public static class IoTTelemetry
{
    /// <summary>Span name for a Integrations.IoT read query.</summary>
    public const string QuerySpan = "Integrations.IoT.query";

    /// <summary>Span name for a Integrations.IoT write command.</summary>
    public const string CommandSpan = "Integrations.IoT.command";

    /// <summary>Span attribute key for the tenant identifier.</summary>
    public const string TenantIdTag = "plantsafety.tenant_id";

    /// <summary>Span attribute key for the site identifier.</summary>
    public const string SiteIdTag = "plantsafety.site_id";

    /// <summary>Span attribute key for the number of records returned.</summary>
    public const string RecordCountTag = "plantsafety.integrations.iot.record_count";

    /// <summary>Span attribute key for whether the result was served from cache.</summary>
    public const string CacheHitTag = "plantsafety.integrations.iot.cache_hit";
}
