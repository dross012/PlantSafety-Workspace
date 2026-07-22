// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Dashboard
//  Layer:          Domain
//  File:           Dashboard.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Dashboards.Dashboard bounded context. Represents a
//                  single logical dashboard page (e.g. "Executive Safety Overview", "Risk
//                  Management", "PPE Compliance") composed of a role-scoped layout of Widget
//                  placements and default filters. Grounded in
//                  PlantSafety_Program_Structure_extract.txt "Dashboard Design Architecture,
//                  1. Dashboard" and "FULL PROGRAM DASHBOARD LIST" (19 named dashboards).
//                  Additive-only evolution.
// ============================================================================
// NOTE: This module's type names intentionally collide with the pre-existing
// Analytics.Dashboard "canonical reference module" (globally `using`-imported in
// GlobalUsing.cs). To avoid CS0104 ambiguity without modifying any pre-existing file, every
// cross-namespace reference below uses an explicit using-alias directive, which always takes
// precedence over using-namespace directives during name resolution.
using DashboardId = PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids.DashboardId;
using DashboardEventId = PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.Ids.DashboardEventId;
using DashboardFilter = PlantSafety.Domain.Dashboards.Dashboard.ValueObjects.DashboardFilter;
using DashboardStatus = PlantSafety.Domain.Dashboards.Dashboard.Enums.DashboardStatus;
using DashboardCategory = PlantSafety.Domain.Dashboards.Dashboard.Enums.DashboardCategory;
using DashboardRole = PlantSafety.Domain.Dashboards.Dashboard.Enums.DashboardRole;
using DashboardWidgetPlacement = PlantSafety.Domain.Dashboards.Dashboard.Entities.DashboardWidgetPlacement;
using IDashboardDomainEvent = PlantSafety.Domain.Dashboards.Dashboard.Events.IDashboardDomainEvent;
using DashboardCreatedEvent = PlantSafety.Domain.Dashboards.Dashboard.Events.DashboardCreatedEvent;
using DashboardPublishedEvent = PlantSafety.Domain.Dashboards.Dashboard.Events.DashboardPublishedEvent;
using DashboardArchivedEvent = PlantSafety.Domain.Dashboards.Dashboard.Events.DashboardArchivedEvent;
using DashboardWidgetAddedEvent = PlantSafety.Domain.Dashboards.Dashboard.Events.DashboardWidgetAddedEvent;
using DashboardWidgetRemovedEvent = PlantSafety.Domain.Dashboards.Dashboard.Events.DashboardWidgetRemovedEvent;
using DashboardLifecycleRules = PlantSafety.Domain.Dashboards.Dashboard.Rules.DashboardLifecycleRules;
using DashboardInvalidStatusTransitionException = PlantSafety.Domain.Dashboards.Dashboard.Exceptions.DashboardInvalidStatusTransitionException;
using DashboardWidgetPlacementNotFoundException = PlantSafety.Domain.Dashboards.Dashboard.Exceptions.DashboardWidgetPlacementNotFoundException;

namespace PlantSafety.Domain.Dashboards.Dashboard.Aggregates;

/// <summary>Aggregate root for the Dashboards.Dashboard bounded context.</summary>
public sealed partial class Dashboard
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this Dashboard aggregate.</summary>
    public DashboardId DashboardId { get; private set; }

    /// <summary>Tenant that owns this dashboard.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this dashboard is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public DashboardStatus Status { get; private set; }

    // ---- Dashboard-specific business properties -----------------------------------

    /// <summary>Human-readable dashboard name (e.g. "Executive Safety Overview").</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Narrative description of this dashboard's purpose.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Catalog category classifying this dashboard among the 19 canonical program dashboards.</summary>
    public DashboardCategory Category { get; private set; } = DashboardCategory.Default;

    /// <summary>Primary role this dashboard is designed for.</summary>
    public DashboardRole Role { get; private set; }

    private readonly List<DashboardFilter> _defaultFilters = [];

    /// <summary>Default filters applied when the dashboard is opened (plant, line, department, shift).</summary>
    public IReadOnlyList<DashboardFilter> DefaultFilters => _defaultFilters.AsReadOnly();

    private readonly List<DashboardWidgetPlacement> _widgetLayout = [];

    /// <summary>Widget placements composing this dashboard's layout.</summary>
    public IReadOnlyList<DashboardWidgetPlacement> WidgetLayout => _widgetLayout.AsReadOnly();

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ------------------------------------------------------

    private readonly List<IDashboardDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IDashboardDomainEvent> DomainEvents
        => new ReadOnlyCollection<IDashboardDomainEvent>(_domainEvents);

    // For rehydration only.
    private Dashboard() { }

    // ---- Factory --------------------------------------------------------------

    /// <summary>Creates a new Dashboard aggregate and raises <see cref="DashboardCreatedEvent"/>.</summary>
    public static Dashboard Create(
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string name,
        string description,
        DashboardCategory category,
        UserId createdBy,
        IEnumerable<DashboardFilter>? defaultFilters = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Dashboard.Name must not be null or whitespace.", nameof(name));

        var effectiveCategory = category ?? DashboardCategory.Default;

        var agg = new Dashboard
        {
            DashboardId  = DashboardId.New(),
            TenantId     = tenantId,
            SiteId       = siteId,
            Status       = DashboardStatus.Draft,
            Name         = name.Trim(),
            Description  = (description ?? string.Empty).Trim(),
            Category     = effectiveCategory,
            Role         = effectiveCategory.PrimaryOwningRole,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        if (defaultFilters is not null)
            agg._defaultFilters.AddRange(defaultFilters);

        agg.AddDomainEvent(new DashboardCreatedEvent(
            DashboardEventId.New(), agg.DashboardId, agg.Category, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior -----------------------------------------------------------

    /// <summary>Publishes a draft dashboard, making it visible to its permitted role(s).</summary>
    public void Publish(UtcTimestamp publishedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!DashboardLifecycleRules.CanPublish(Status))
            throw new DashboardInvalidStatusTransitionException(DashboardId, Status.ToString(), DashboardStatus.Published.ToString(), correlationId, causationId);

        Status = DashboardStatus.Published;

        AddDomainEvent(new DashboardPublishedEvent(DashboardEventId.New(), DashboardId, publishedAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Archives a dashboard, removing it from navigation.</summary>
    public void Archive(UtcTimestamp archivedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!DashboardLifecycleRules.CanArchive(Status))
            throw new DashboardInvalidStatusTransitionException(DashboardId, Status.ToString(), DashboardStatus.Archived.ToString(), correlationId, causationId);

        Status = DashboardStatus.Archived;

        AddDomainEvent(new DashboardArchivedEvent(DashboardEventId.New(), DashboardId, archivedAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Adds a Widget placement to this dashboard's layout.</summary>
    public void AddWidget(
        PlantSafety.Domain.Dashboards.Widget.ValueObjects.Ids.WidgetId widgetId,
        int row,
        int column,
        int rowSpan,
        int columnSpan,
        IReadOnlyList<DashboardFilter>? filterOverrides,
        UtcTimestamp addedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        var placement = new DashboardWidgetPlacement(Guid.NewGuid(), DashboardId, widgetId, row, column, rowSpan, columnSpan, filterOverrides);
        _widgetLayout.Add(placement);

        AddDomainEvent(new DashboardWidgetAddedEvent(DashboardEventId.New(), DashboardId, widgetId.Value, addedAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Removes a Widget placement from this dashboard's layout.</summary>
    public void RemoveWidget(
        PlantSafety.Domain.Dashboards.Widget.ValueObjects.Ids.WidgetId widgetId,
        UtcTimestamp removedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        var placement = _widgetLayout.FirstOrDefault(p => p.WidgetId.Equals(widgetId));
        if (placement is null)
            throw new DashboardWidgetPlacementNotFoundException(DashboardId, widgetId.Value, correlationId, causationId);

        _widgetLayout.Remove(placement);

        AddDomainEvent(new DashboardWidgetRemovedEvent(DashboardEventId.New(), DashboardId, widgetId.Value, removedAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Replaces this dashboard's default filters.</summary>
    public void SetDefaultFilters(IEnumerable<DashboardFilter> filters)
    {
        _defaultFilters.Clear();
        _defaultFilters.AddRange(filters);
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IDashboardDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
