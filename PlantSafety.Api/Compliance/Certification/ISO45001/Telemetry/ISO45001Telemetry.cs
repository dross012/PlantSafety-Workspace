// ============================================================================
// File: ISO45001Telemetry.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Telemetry span name and OpenTelemetry attribute key constants for the ISO45001 module.
// ============================================================================
//
// Onboarding Notes:
// - Referenced by OpenTelemetry instrumentation. Infrastructure registers ActivitySource instances; this file only declares the name constants.
//
// Rationale:
// - Centralising tag keys prevents typos in distributed trace attributes and ensures consistent filtering in Azure Monitor and Jaeger.
//
// Future Extension Seams:
// - Add custom span attribute keys (TenantId, SiteId, RecordCount) as ISO45001 telemetry requirements expand.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Certification.ISO45001.Telemetry;

/// <summary>Telemetry span names and OpenTelemetry attribute key constants for ISO45001.</summary>
public static class ISO45001Telemetry
{
    /// <summary>Span name for a ISO45001 read query.</summary>
    public const string QuerySpan = "Compliance.Certification.ISO45001LC_.ISO45001LC_.query";

    /// <summary>Span name for a ISO45001 write command.</summary>
    public const string CommandSpan = "Compliance.Certification.ISO45001LC_.ISO45001LC_.command";

    /// <summary>Span attribute key for the tenant identifier.</summary>
    public const string TenantIdTag = "plantsafety.tenant_id";

    /// <summary>Span attribute key for the site identifier.</summary>
    public const string SiteIdTag = "plantsafety.site_id";

    /// <summary>Span attribute key for the number of records returned.</summary>
    public const string RecordCountTag = "plantsafety.Compliance.Certification.ISO45001LC_.record_count";

    /// <summary>Span attribute key for whether the result was served from cache.</summary>
    public const string CacheHitTag = "plantsafety.Compliance.Certification.ISO45001LC_.cache_hit";
}