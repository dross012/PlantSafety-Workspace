// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// UPDATED: 2026-07-15 — repointed at the new dedicated Environmental Health
//   monitoring-reading endpoint (GET v1/features/environmental-health/readings)
//   now that PlantSafety.Domain/Application/Api/Infrastructure own a real
//   EnvironmentalHealth aggregate. Previously this module borrowed the
//   Compliance/Policy/Environmental read-model as a stand-in.
// FILE DESCRIPTION: Client-side DTOs for the Environmental Health module.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.EnvironmentalHealth.Models;

/// <summary>Represents a single environmental monitoring reading (mirrors <c>EnvironmentalHealthResponse</c>).</summary>
public sealed class EnvironmentalHealthItemDto
{
    /// <summary>Gets the reading identifier.</summary>
    public Guid EnvironmentalHealthId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the environmental metric type (e.g., AirQuality, NoiseLevel, WaterQuality, Emissions, WasteHandling).</summary>
    public string MetricType { get; init; } = string.Empty;

    /// <summary>Gets the measured value for the metric.</summary>
    public decimal MeasurementValue { get; init; }

    /// <summary>Gets the unit of measure for <see cref="MeasurementValue"/>.</summary>
    public string Unit { get; init; } = string.Empty;

    /// <summary>Gets the regulatory/internal threshold this measurement is evaluated against.</summary>
    public decimal Threshold { get; init; }

    /// <summary>Gets the evaluated status (Normal/Warning/Critical).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp the reading was recorded.</summary>
    public DateTimeOffset RecordedAtUtc { get; init; }
}

/// <summary>Paginated envelope returned by GET v1/features/environmental-health/readings (mirrors <c>EnvironmentalHealthListResponse</c>).</summary>
public sealed class EnvironmentalHealthResponseDto
{
    /// <summary>Gets the environmental health readings in the requested window.</summary>
    public IReadOnlyList<EnvironmentalHealthItemDto> Items { get; init; } = [];

    /// <summary>Gets the total matching record count across all pages.</summary>
    public int TotalCount { get; init; }

    /// <summary>Gets the current page number.</summary>
    public int PageNumber { get; init; }

    /// <summary>Gets the page size.</summary>
    public int PageSize { get; init; }

    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages { get; init; }
}
