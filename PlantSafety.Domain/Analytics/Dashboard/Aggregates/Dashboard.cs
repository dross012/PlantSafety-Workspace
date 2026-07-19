// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Domain
//  File:           Dashboard.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  KPI-dashboard record shape grounded in project_plantsafety_domains.md
//                  Module 8 (Dashboard: real-time KPI visualization and operational
//                  metrics; KPIs: dashboard load latency, data freshness; consumes
//                  events from all modules, does not publish).
//                  Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Dashboard bounded context. Tracks a single
//                  KPI dashboard definition: its name, type, owner, refresh cadence,
//                  and widget composition.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Dashboard.Aggregates;

/// <summary>Aggregate root for the Dashboard bounded context.</summary>
public sealed partial class Dashboard
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Dashboard aggregate.</summary>
    public DashboardId DashboardId { get; private set; }

    /// <summary>Tenant that owns this dashboard record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this dashboard is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public DashboardStatus Status { get; private set; }

    // ---- Dashboard-specific business properties -------------------------------------

    /// <summary>Human-readable dashboard name.</summary>
    public string DashboardName { get; private set; } = string.Empty;

    /// <summary>Dashboard classification (e.g., Executive, Operator, SiteSafety, Compliance).</summary>
    public string DashboardType { get; private set; } = string.Empty;

    /// <summary>Identity of the accountable dashboard owner.</summary>
    public UserId OwnerId { get; private set; }

    /// <summary>Refresh interval, in seconds, for live data widgets.</summary>
    public int RefreshIntervalSeconds { get; private set; } = 60;

    /// <summary>Count of widgets configured on this dashboard.</summary>
    public int WidgetCount { get; private set; }

    /// <summary>Indicates whether this dashboard is published for general use.</summary>
    public bool IsPublished { get; private set; }

    /// <summary>UTC timestamp this dashboard's data was last refreshed.</summary>
    public UtcTimestamp? LastRefreshedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IDashboardDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IDashboardDomainEvent> DomainEvents
        => new ReadOnlyCollection<IDashboardDomainEvent>(_domainEvents);

    // For rehydration only.
    private Dashboard() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Dashboard aggregate and raises <see cref="DashboardCreatedEvent"/>.</summary>
    public static Dashboard Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string dashboardName = "",
        string dashboardType = "",
        UserId ownerId = default,
        int refreshIntervalSeconds = 60,
        int widgetCount = 0,
        bool isPublished = false)
    {
        var agg = new Dashboard
        {
            DashboardId              = DashboardId.New(),
            TenantId                 = tenantId,
            SiteId                   = siteId,
            Status                   = DashboardStatus.Created,
            DashboardName            = (dashboardName ?? string.Empty).Trim(),
            DashboardType            = (dashboardType ?? string.Empty).Trim(),
            OwnerId                  = ownerId,
            RefreshIntervalSeconds   = refreshIntervalSeconds < 1 ? 1 : refreshIntervalSeconds,
            WidgetCount              = widgetCount < 0 ? 0 : widgetCount,
            IsPublished              = isPublished,
            CreatedAtUtc             = createdAtUtc,
            CreatedBy                = createdBy,
        };

        agg.AddDomainEvent(new DashboardCreatedEvent(
            EventId:       new DashboardEventId(Uuid7.New()),
            DashboardId:      agg.DashboardId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IDashboardDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params DashboardStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Dashboard {DashboardId}.");
    }
}
