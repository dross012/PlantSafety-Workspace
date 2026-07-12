// ============================================================================
// File: TextingTelemetry.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Telemetry span name and OpenTelemetry attribute key constants for the Texting module.
// ============================================================================
//
// Onboarding Notes:
// - Referenced by OpenTelemetry instrumentation. Infrastructure registers ActivitySource instances; this file only declares the name constants.
//
// Rationale:
// - Centralising tag keys prevents typos in distributed trace attributes and ensures consistent filtering in Azure Monitor and Jaeger.
//
// Future Extension Seams:
// - Add custom span attribute keys (TenantId, SiteId, RecordCount) as Texting telemetry requirements expand.
//
// ============================================================================
namespace PlantSafety.Api.Integrations.Notifications.Texting.Telemetry;

/// <summary>Telemetry span names and OpenTelemetry attribute key constants for Texting.</summary>
public static class TextingTelemetry
{
    /// <summary>Span name for a Texting read query.</summary>
    public const string QuerySpan = "Integrations.Notifications.TextingLC_.TextingLC_.query";

    /// <summary>Span name for a Texting write command.</summary>
    public const string CommandSpan = "Integrations.Notifications.TextingLC_.TextingLC_.command";

    /// <summary>Span attribute key for the tenant identifier.</summary>
    public const string TenantIdTag = "plantsafety.tenant_id";

    /// <summary>Span attribute key for the site identifier.</summary>
    public const string SiteIdTag = "plantsafety.site_id";

    /// <summary>Span attribute key for the number of records returned.</summary>
    public const string RecordCountTag = "plantsafety.Integrations.Notifications.TextingLC_.record_count";

    /// <summary>Span attribute key for whether the result was served from cache.</summary>
    public const string CacheHitTag = "plantsafety.Integrations.Notifications.TextingLC_.cache_hit";
}