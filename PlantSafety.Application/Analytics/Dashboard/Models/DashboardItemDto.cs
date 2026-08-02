// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Application
//  File:           DashboardItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single Dashboard record. Backs
//                  GetDashboardByIdQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto's shape.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Analytics.Dashboard.Models;

/// <summary>A single Dashboard read-model record.</summary>
public sealed record DashboardItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid DashboardId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Human-readable dashboard name.</summary>
    public required string DashboardName { get; init; }

    /// <summary>Dashboard classification (e.g., Executive, Operator, SiteSafety, Compliance).</summary>
    public required string DashboardType { get; init; }

    /// <summary>Identity of the accountable dashboard owner.</summary>
    public Guid? OwnerId { get; init; }

    /// <summary>Refresh interval, in seconds, for live data widgets.</summary>
    public int RefreshIntervalSeconds { get; init; }

    /// <summary>Count of widgets configured on this dashboard.</summary>
    public int WidgetCount { get; init; }

    /// <summary>Indicates whether this dashboard is published for general use.</summary>
    public bool IsPublished { get; init; }

    /// <summary>UTC timestamp this dashboard's data was last refreshed.</summary>
    public DateTime? LastRefreshedAtUtc { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}
