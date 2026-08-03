// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Domain
//  File:           ProductionLine.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the ProductionLine master-data bounded context -- the
//                  genuinely-new, deliberately minimal piece of this module (see
//                  docs/Phase5/ui-gaps-scoping.md "ProductionTracking" section: "a legitimately
//                  new, small master-data concept the existing telemetry value objects can be
//                  recorded against"). Tracks only identity, display name, site scope, and a
//                  coarse operating status; everything else (throughput, KPIs, bottlenecks) is
//                  composed at query time from ProductionRun data plus the real Phase 5
//                  Observability.Telemetry/Analytics.Dashboard.Kpi infrastructure. Mirrors
//                  PlantSafety.Domain.Operational.WorkOrders.Aggregates.Part's simplicity level.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.ProductionTracking.Enums;
using PlantSafety.Domain.Operational.ProductionTracking.Events;
using PlantSafety.Domain.Operational.ProductionTracking.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.ProductionTracking.Aggregates;

/// <summary>Aggregate root for the ProductionLine (master-data) bounded context.</summary>
public sealed class ProductionLine
{
    /// <summary>Strong-typed identity of this ProductionLine aggregate.</summary>
    public ProductionLineId ProductionLineId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Human-readable line code (format LINE-###).</summary>
    public string LineCode { get; private set; } = string.Empty;

    /// <summary>Line display name.</summary>
    public string LineName { get; private set; } = string.Empty;

    /// <summary>Real-time operating status.</summary>
    public ProductionLineStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user/system that created this record.</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>UTC instant of the most recent update to this record.</summary>
    public UtcTimestamp UpdatedAtUtc { get; private set; }

    private readonly List<IProductionTrackingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IProductionTrackingDomainEvent> DomainEvents
        => new ReadOnlyCollection<IProductionTrackingDomainEvent>(_domainEvents);

    // For rehydration only.
    private ProductionLine() { }

    /// <summary>Registers a new ProductionLine master-data record.</summary>
    public static ProductionLine Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        Guid tenantId,
        Guid? siteId,
        string lineCode,
        string lineName)
    {
        if (string.IsNullOrWhiteSpace(lineCode))
            throw new ArgumentException("LineCode must be supplied.", nameof(lineCode));
        if (string.IsNullOrWhiteSpace(lineName))
            throw new ArgumentException("LineName must be supplied.", nameof(lineName));

        var agg = new ProductionLine
        {
            ProductionLineId = ProductionLineId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            LineCode = lineCode.Trim(),
            LineName = lineName.Trim(),
            Status = ProductionLineStatus.Idle,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
            UpdatedAtUtc = createdAtUtc,
        };

        agg.AddDomainEvent(new ProductionLineRegisteredEvent(
            EventId: Guid.NewGuid(),
            ProductionLineId: agg.ProductionLineId,
            LineCode: agg.LineCode,
            CreatedAtUtc: createdAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return agg;
    }

    /// <summary>Updates the line's real-time operating status.</summary>
    public void UpdateStatus(ProductionLineStatus status, UtcTimestamp occurredAtUtc)
    {
        Status = status;
        UpdatedAtUtc = occurredAtUtc;
    }

    private void AddDomainEvent(IProductionTrackingDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
