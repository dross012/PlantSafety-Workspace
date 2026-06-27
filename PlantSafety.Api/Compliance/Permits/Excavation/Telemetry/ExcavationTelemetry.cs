// ============================================================================
// File: ExcavationTelemetry.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Telemetry span name and OpenTelemetry attribute key constants for the Excavation module.
// ============================================================================
//
// Onboarding Notes:
// - Referenced by OpenTelemetry instrumentation. Infrastructure registers ActivitySource instances; this file only declares the name constants.
//
// Rationale:
// - Centralising tag keys prevents typos in distributed trace attributes and ensures consistent filtering in Azure Monitor and Jaeger.
//
// Future Extension Seams:
// - Add custom span attribute keys (TenantId, SiteId, RecordCount) as Excavation telemetry requirements expand.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Permits.Excavation.Telemetry;

/// <summary>Telemetry span names and OpenTelemetry attribute key constants for Excavation.</summary>
public static class ExcavationTelemetry
{
    /// <summary>Span name for a Excavation read query.</summary>
    public const string QuerySpan = "Compliance.Permits.ExcavationLC_.ExcavationLC_.query";

    /// <summary>Span name for a Excavation write command.</summary>
    public const string CommandSpan = "Compliance.Permits.ExcavationLC_.ExcavationLC_.command";

    /// <summary>Span attribute key for the tenant identifier.</summary>
    public const string TenantIdTag = "plantsafety.tenant_id";

    /// <summary>Span attribute key for the site identifier.</summary>
    public const string SiteIdTag = "plantsafety.site_id";

    /// <summary>Span attribute key for the number of records returned.</summary>
    public const string RecordCountTag = "plantsafety.Compliance.Permits.ExcavationLC_.record_count";

    /// <summary>Span attribute key for whether the result was served from cache.</summary>
    public const string CacheHitTag = "plantsafety.Compliance.Permits.ExcavationLC_.cache_hit";
}