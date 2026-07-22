// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Dashboards.Widget
//  Layer:          Domain
//  File:           Widget.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Dashboards.Widget bounded context. Represents a single
//                  reusable, module-agnostic dashboard visualization component (line chart, bar
//                  chart, pie chart, gauge, table, heatmap, facility map, timeline, trend graph,
//                  or KPI card), configured with a data source, parameters, refresh cadence, and
//                  role-based visibility permissions. Grounded in
//                  PlantSafety_Program_Structure_extract.txt "Dashboard Design Architecture,
//                  2. Widget" and "5. Widget Library (Reusable Components)".
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Dashboards.Widget.Enums;
using PlantSafety.Domain.Dashboards.Widget.Events;
using PlantSafety.Domain.Dashboards.Widget.Exceptions;
using PlantSafety.Domain.Dashboards.Widget.Rules;
using PlantSafety.Domain.Dashboards.Widget.ValueObjects;
using PlantSafety.Domain.Dashboards.Widget.ValueObjects.Ids;

namespace PlantSafety.Domain.Dashboards.Widget.Aggregates;

/// <summary>Aggregate root for the Dashboards.Widget bounded context.</summary>
public sealed partial class Widget
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this Widget aggregate.</summary>
    public WidgetId WidgetId { get; private set; }

    /// <summary>Tenant that owns this widget definition.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public WidgetStatus Status { get; private set; }

    // ---- Widget-specific business properties -----------------------------------

    /// <summary>Human-readable widget name (e.g. "TRIR Trend Line").</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Visualization type (LineChart/BarChart/PieChart/Gauge/Table/Heatmap/Map/Timeline/TrendGraph/KpiCard).</summary>
    public WidgetType Type { get; private set; } = WidgetType.Default;

    /// <summary>Owning module and query/endpoint this widget resolves its data from.</summary>
    public WidgetDataSource DataSource { get; private set; }

    /// <summary>Refresh cadence for this widget's data.</summary>
    public WidgetRefreshInterval RefreshInterval { get; private set; } = WidgetRefreshInterval.HeavyMetric;

    private readonly List<WidgetParameter> _parameters = [];

    /// <summary>Configuration parameters (time range, plant, module, thresholds).</summary>
    public IReadOnlyList<WidgetParameter> Parameters => _parameters.AsReadOnly();

    private readonly List<string> _permittedRoles = [];

    /// <summary>Canonical role names permitted to view this widget (empty = visible to all authorized dashboard viewers).</summary>
    public IReadOnlyList<string> PermittedRoles => _permittedRoles.AsReadOnly();

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ------------------------------------------------------

    private readonly List<IWidgetDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IWidgetDomainEvent> DomainEvents
        => new ReadOnlyCollection<IWidgetDomainEvent>(_domainEvents);

    // For rehydration only.
    private Widget() { }

    // ---- Factory --------------------------------------------------------------

    /// <summary>Creates a new Widget aggregate and raises <see cref="WidgetCreatedEvent"/>.</summary>
    public static Widget Create(
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        string name,
        WidgetType type,
        WidgetDataSource dataSource,
        UserId createdBy,
        WidgetRefreshInterval? refreshInterval = null,
        IEnumerable<WidgetParameter>? parameters = null,
        IEnumerable<string>? permittedRoles = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Widget.Name must not be null or whitespace.", nameof(name));

        var agg = new Widget
        {
            WidgetId        = WidgetId.New(),
            TenantId        = tenantId,
            Status          = WidgetStatus.Draft,
            Name            = name.Trim(),
            Type            = type ?? WidgetType.Default,
            DataSource      = dataSource,
            RefreshInterval = refreshInterval ?? WidgetRefreshInterval.HeavyMetric,
            CreatedAtUtc    = createdAtUtc,
            CreatedBy       = createdBy,
        };

        if (parameters is not null)
            agg._parameters.AddRange(parameters);

        if (permittedRoles is not null)
            agg._permittedRoles.AddRange(permittedRoles.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim()));

        agg.AddDomainEvent(new WidgetCreatedEvent(
            WidgetEventId.New(), agg.WidgetId, agg.Type, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior -----------------------------------------------------------

    /// <summary>Activates a draft widget, making it available for placement on dashboards.</summary>
    public void Activate(UtcTimestamp activatedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!WidgetLifecycleRules.CanActivate(Status))
            throw new WidgetInvalidStatusTransitionException(WidgetId, Status.ToString(), WidgetStatus.Active.ToString(), correlationId, causationId);

        Status = WidgetStatus.Active;

        AddDomainEvent(new WidgetActivatedEvent(WidgetEventId.New(), WidgetId, activatedAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Reconfigures the widget's data source, refresh interval, and/or parameters.</summary>
    public void Configure(
        WidgetDataSource? dataSource,
        WidgetRefreshInterval? refreshInterval,
        IEnumerable<WidgetParameter>? parameters,
        UtcTimestamp configuredAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (dataSource is not null)
            DataSource = dataSource.Value;

        if (refreshInterval is not null)
            RefreshInterval = refreshInterval.Value;

        if (parameters is not null)
        {
            _parameters.Clear();
            _parameters.AddRange(parameters);
        }

        AddDomainEvent(new WidgetConfiguredEvent(WidgetEventId.New(), WidgetId, configuredAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Deprecates an active widget.</summary>
    public void Deprecate(string reason, UtcTimestamp deprecatedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!WidgetLifecycleRules.CanDeprecate(Status))
            throw new WidgetInvalidStatusTransitionException(WidgetId, Status.ToString(), WidgetStatus.Deprecated.ToString(), correlationId, causationId);

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason must not be null or whitespace.", nameof(reason));

        Status = WidgetStatus.Deprecated;

        AddDomainEvent(new WidgetDeprecatedEvent(WidgetEventId.New(), WidgetId, reason.Trim(), deprecatedAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IWidgetDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
