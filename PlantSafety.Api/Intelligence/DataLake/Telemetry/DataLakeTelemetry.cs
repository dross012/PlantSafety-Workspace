// ============================================================================
// File: DataLakeTelemetry.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Telemetry span name and OpenTelemetry attribute key constants for the DataLake module.
// ============================================================================
//
// Onboarding Notes:
// - Referenced by OpenTelemetry instrumentation. Infrastructure registers ActivitySource instances; this file only declares the name constants.
//
// Rationale:
// - Centralising tag keys prevents typos in distributed trace attributes and ensures consistent filtering in Azure Monitor and Jaeger.
//
// Future Extension Seams:
// - Add custom span attribute keys (TenantId, SiteId, RecordCount) as DataLake telemetry requirements expand.
//
// ============================================================================
namespace PlantSafety.Api.Intelligence.DataLake.Telemetry;

/// <summary>Telemetry span names and OpenTelemetry attribute key constants for DataLake.</summary>
public static class DataLakeTelemetry
{
    /// <summary>Span name for a DataLake read query.</summary>
    public const string QuerySpan = "Intelligence.DataLakeLC_.DataLakeLC_.query";

    /// <summary>Span name for a DataLake write command.</summary>
    public const string CommandSpan = "Intelligence.DataLakeLC_.DataLakeLC_.command";

    /// <summary>Span attribute key for the tenant identifier.</summary>
    public const string TenantIdTag = "plantsafety.tenant_id";

    /// <summary>Span attribute key for the site identifier.</summary>
    public const string SiteIdTag = "plantsafety.site_id";

    /// <summary>Span attribute key for the number of records returned.</summary>
    public const string RecordCountTag = "plantsafety.Intelligence.DataLakeLC_.record_count";

    /// <summary>Span attribute key for whether the result was served from cache.</summary>
    public const string CacheHitTag = "plantsafety.Intelligence.DataLakeLC_.cache_hit";
}