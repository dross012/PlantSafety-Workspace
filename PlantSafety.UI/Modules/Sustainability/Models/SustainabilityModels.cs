// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Sustainability module. No
//   PlantSafety.Api read-model endpoint exists yet for this module today —
//   PlantSafety.Domain/Sustainability is a thin Id/CreatedAtUtc/CreatedBy
//   scaffold aggregate with no queries/handlers/controllers wired. This
//   shape (emissions/energy/water/waste metric records) is the
//   best-inferable read-model DTO following the sibling Intelligence
//   read-model conventions (TenantId/SiteId/GeneratedAt/Items envelope with
//   a From/To date window) so the UI is ready to bind the instant a real
//   endpoint ships. Until then, SustainabilityApiClient calls gracefully
//   fault to the page's Error state.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Sustainability.Models;

/// <summary>Represents a single sustainability metric record (emissions/energy/water/waste).</summary>
public sealed class SustainabilityMetricDto
{
    /// <summary>Gets the unique identifier of the metric record.</summary>
    public Guid MetricId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the display name of the metric.</summary>
    public string MetricName { get; init; } = string.Empty;

    /// <summary>Gets the metric category (e.g., Emissions, Energy, Water, Waste).</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Gets the measured value for the reporting period.</summary>
    public double Value { get; init; }

    /// <summary>Gets the unit of measure (e.g., tCO2e, kWh, m³, kg).</summary>
    public string Unit { get; init; } = string.Empty;

    /// <summary>Gets the target value for the reporting period, or null when no target is set.</summary>
    public double? Target { get; init; }

    /// <summary>Gets the UTC timestamp marking the end of the reporting period.</summary>
    public DateTime PeriodEndUtc { get; init; }

    /// <summary>Gets the current status against target (e.g., OnTrack/AtRisk/Exceeded).</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>Envelope for the (not-yet-wired) GET v1/sustainability/metrics endpoint.</summary>
public sealed class SustainabilityResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the sustainability metric items in the requested window.</summary>
    public IReadOnlyList<SustainabilityMetricDto> Items { get; init; } = [];
}
