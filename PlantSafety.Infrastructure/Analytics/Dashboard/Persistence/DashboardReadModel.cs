// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Infrastructure
//  File:           DashboardReadModel.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper read model DTO for Dashboard queries.
//                  Additive-only evolution.
//  UPDATED:        2026-08-02 -- Phase 4 full build-out: added the full row shape (DashboardName/
//                  DashboardType/OwnerId/RefreshIntervalSeconds/WidgetCount/IsPublished/
//                  LastRefreshedAtUtc/lifecycle columns/ETag). usp_Dashboard_GetById/GetAll did not
//                  exist anywhere in the codebase before this pass even though this class referenced
//                  them by name -- GetByIdAsync/GetAllAsync could never have succeeded against a
//                  real database until now.
// ============================================================================
namespace PlantSafety.Infrastructure.Analytics.Dashboard.Persistence;

/// <summary>Dapper read model returned by Dashboard query stored procedures.</summary>
public sealed record DashboardReadModel
{
    /// <summary>Aggregate identifier.</summary>
    public Guid DashboardId { get; init; }

    /// <summary>Current lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Tenant scoping identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Optional site scoping identifier.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable dashboard name.</summary>
    public string DashboardName { get; init; } = string.Empty;

    /// <summary>Dashboard classification (e.g., Executive, Operator, SiteSafety, Compliance).</summary>
    public string DashboardType { get; init; } = string.Empty;

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

    /// <summary>Identity of the user who published this dashboard, if published.</summary>
    public Guid? PublishedByUserId { get; init; }

    /// <summary>UTC instant the dashboard was published.</summary>
    public DateTime? PublishedAtUtc { get; init; }

    /// <summary>Identity of the user who deprecated this dashboard, if deprecated.</summary>
    public Guid? DeprecatedByUserId { get; init; }

    /// <summary>UTC instant the dashboard was deprecated.</summary>
    public DateTime? DeprecatedAtUtc { get; init; }

    /// <summary>Identity of the user who closed this dashboard, if closed.</summary>
    public Guid? ClosedByUserId { get; init; }

    /// <summary>UTC instant the dashboard was closed.</summary>
    public DateTime? ClosedAtUtc { get; init; }

    /// <summary>UTC instant the dashboard was archived.</summary>
    public DateTime? ArchivedAtUtc { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the creating user.</summary>
    public Guid CreatedBy { get; init; }

    /// <summary>UTC timestamp when the record was last updated, if ever.</summary>
    public DateTime? UpdatedAtUtc { get; init; }

    /// <summary>Identity of the user who last updated the record, if ever.</summary>
    public Guid? UpdatedBy { get; init; }

    /// <summary>Optimistic-concurrency token (native RowVersion projected as a hex string).</summary>
    public string? ETag { get; init; }
}
