// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargePoint
//  Layer:          Domain
//  File:           DischargePoint.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a permitted outfall/discharge point tied to a
//                  DischargePermit. Models registration, deactivation, reactivation, and
//                  decommissioning. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Aggregates;

/// <summary>Aggregate root for a permitted outfall/discharge point.</summary>
public sealed partial class DischargePoint
{
    /// <summary>Strong-typed identity of this DischargePoint aggregate.</summary>
    public DischargePointId DischargePointId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public DischargePointStatus Status { get; private set; }

    /// <summary>Identity of the owning DischargePermit aggregate.</summary>
    public Guid DischargePermitId { get; private set; }

    /// <summary>Facility-assigned discharge point code (e.g. "Outfall 001").</summary>
    public string PointCode { get; private set; } = string.Empty;

    /// <summary>Descriptive name of the discharge point.</summary>
    public string PointName { get; private set; } = string.Empty;

    /// <summary>Name of the receiving water body.</summary>
    public string ReceivingWaterBody { get; private set; } = string.Empty;

    /// <summary>Outfall configuration classification.</summary>
    public OutfallType Outfall { get; private set; }

    /// <summary>Latitude of the discharge point, if geocoded.</summary>
    public decimal? Latitude { get; private set; }

    /// <summary>Longitude of the discharge point, if geocoded.</summary>
    public decimal? Longitude { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    private readonly List<IDischargePointDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IDischargePointDomainEvent> DomainEvents
        => new ReadOnlyCollection<IDischargePointDomainEvent>(_domainEvents);

    private DischargePoint() { }

    /// <summary>Registers a new discharge point and raises <see cref="DischargePointRegisteredEvent"/>.</summary>
    public static DischargePoint Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid dischargePermitId = default,
        string pointCode = "",
        string pointName = "",
        string receivingWaterBody = "",
        OutfallType outfall = OutfallType.Direct,
        decimal? latitude = null,
        decimal? longitude = null)
    {
        var agg = new DischargePoint
        {
            DischargePointId = DischargePointId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = DischargePointStatus.Active,
            DischargePermitId = dischargePermitId,
            PointCode = (pointCode ?? string.Empty).Trim(),
            PointName = (pointName ?? string.Empty).Trim(),
            ReceivingWaterBody = (receivingWaterBody ?? string.Empty).Trim(),
            Outfall = outfall,
            Latitude = latitude,
            Longitude = longitude,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new DischargePointRegisteredEvent(
            DischargePointEventId.New(), agg.DischargePointId, dischargePermitId, agg.PointCode, outfall, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    /// <summary>Deactivates the discharge point.</summary>
    public void Deactivate(string? reason, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!DischargePointLifecycleRules.CanDeactivate(Status))
            throw new DischargePointInvalidStatusTransitionException(DischargePointId, Status.ToString(), DischargePointStatus.Inactive.ToString(), correlationId, causationId);

        Status = DischargePointStatus.Inactive;

        AddDomainEvent(new DischargePointDeactivatedEvent(
            DischargePointEventId.New(), DischargePointId, reason, occurredAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Reactivates the discharge point.</summary>
    public void Reactivate(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!DischargePointLifecycleRules.CanReactivate(Status))
            throw new DischargePointInvalidStatusTransitionException(DischargePointId, Status.ToString(), DischargePointStatus.Active.ToString(), correlationId, causationId);

        Status = DischargePointStatus.Active;

        AddDomainEvent(new DischargePointReactivatedEvent(
            DischargePointEventId.New(), DischargePointId, occurredAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Decommissions the discharge point.</summary>
    public void Decommission(string? reason, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!DischargePointLifecycleRules.CanDecommission(Status))
            throw new DischargePointInvalidStatusTransitionException(DischargePointId, Status.ToString(), DischargePointStatus.Decommissioned.ToString(), correlationId, causationId);

        Status = DischargePointStatus.Decommissioned;

        AddDomainEvent(new DischargePointDecommissionedEvent(
            DischargePointEventId.New(), DischargePointId, reason, occurredAtUtc, actor, correlationId, causationId));
    }

    private void AddDomainEvent(IDischargePointDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}