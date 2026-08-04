// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.OperatingGeofence
//  Layer:          Domain
//  File:           OperatingGeofence.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single operating-domain geofence (exclusion, restricted,
//                  operating, or speed-limit zone) constraining autonomous vehicle/mobile-robot
//                  operation on-site. Models definition, activation, violation detection, scheduled
//                  audits after site layout changes, and retirement. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.OperatingGeofence.Aggregates;

/// <summary>Aggregate root for a single operating-domain geofence.</summary>
public sealed partial class OperatingGeofence
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this OperatingGeofence aggregate.</summary>
    public OperatingGeofenceId OperatingGeofenceId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public OperatingGeofenceStatus Status { get; private set; }

    // ---- Geofence definition ----------------------------------------------------

    /// <summary>Human-readable name of this geofence.</summary>
    public string GeofenceName { get; private set; } = string.Empty;

    /// <summary>Zone classification.</summary>
    public GeofenceZoneType ZoneType { get; private set; }

    /// <summary>Boundary definition (GeoJSON/WKT polygon or equivalent site-coordinate encoding).</summary>
    public string BoundaryDefinition { get; private set; } = string.Empty;

    /// <summary>Enforced maximum speed within the zone, if applicable.</summary>
    public decimal? MaxSpeedKph { get; private set; }

    /// <summary>Cumulative count of violations detected against this geofence.</summary>
    public int ViolationCount { get; private set; }

    /// <summary>UTC timestamp of the most recent audit, if any.</summary>
    public UtcTimestamp? LastAuditedAtUtc { get; private set; }

    /// <summary>UTC timestamp the next audit is due, if scheduled.</summary>
    public UtcTimestamp? NextAuditDueAtUtc { get; private set; }

    // ---- Audit -------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -------------------------------------------------------

    private readonly List<IOperatingGeofenceDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IOperatingGeofenceDomainEvent> DomainEvents
        => new ReadOnlyCollection<IOperatingGeofenceDomainEvent>(_domainEvents);

    // For rehydration only.
    private OperatingGeofence() { }

    // ---- Factory: Create (Define) ---------------------------------------------

    /// <summary>Defines a new operating-domain geofence and raises <see cref="OperatingGeofenceDefinedEvent"/>.</summary>
    public static OperatingGeofence Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string geofenceName = "",
        GeofenceZoneType zoneType = GeofenceZoneType.OperatingZone,
        string boundaryDefinition = "",
        decimal? maxSpeedKph = null)
    {
        var agg = new OperatingGeofence
        {
            OperatingGeofenceId = OperatingGeofenceId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = OperatingGeofenceStatus.Draft,
            GeofenceName = (geofenceName ?? string.Empty).Trim(),
            ZoneType = zoneType,
            BoundaryDefinition = (boundaryDefinition ?? string.Empty).Trim(),
            MaxSpeedKph = maxSpeedKph,
            ViolationCount = 0,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new OperatingGeofenceDefinedEvent(
            OperatingGeofenceEventId.New(), agg.OperatingGeofenceId, zoneType, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: Activate -----------------------------------------------------

    /// <summary>Activates the geofence for enforcement.</summary>
    public void Activate(PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(OperatingGeofenceStatus.Draft);
        Status = OperatingGeofenceStatus.Active;
    }

    // ---- Behavior: RecordViolation -----------------------------------------------

    /// <summary>Records a geofence violation detected against an autonomous asset.</summary>
    public void RecordViolation(Guid autonomousVehicleAssetId, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(OperatingGeofenceStatus.Active);

        ViolationCount++;

        AddDomainEvent(new GeofenceViolationDetectedEvent(
            OperatingGeofenceEventId.New(), OperatingGeofenceId, autonomousVehicleAssetId, ViolationCount, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: ScheduleAudit / RecordAudit ------------------------------------

    /// <summary>Schedules the next geofence audit (e.g. after a site layout change).</summary>
    public void ScheduleAudit(UtcTimestamp dueAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(OperatingGeofenceStatus.Active, OperatingGeofenceStatus.UnderAudit);

        NextAuditDueAtUtc = dueAtUtc;
        Status = OperatingGeofenceStatus.UnderAudit;
    }

    /// <summary>Records completion of a geofence audit.</summary>
    public void RecordAudit(UtcTimestamp auditedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(OperatingGeofenceStatus.Active, OperatingGeofenceStatus.UnderAudit);

        LastAuditedAtUtc = auditedAtUtc;
        NextAuditDueAtUtc = null;
        Status = OperatingGeofenceStatus.Active;

        AddDomainEvent(new OperatingGeofenceAuditedEvent(
            OperatingGeofenceEventId.New(), OperatingGeofenceId, auditedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Retire -------------------------------------------------------

    /// <summary>Retires the geofence.</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == OperatingGeofenceStatus.Retired)
            throw new OperatingGeofenceInvalidStatusTransitionException(OperatingGeofenceId, Status.ToString(), OperatingGeofenceStatus.Retired.ToString(), correlationId, causationId);

        Status = OperatingGeofenceStatus.Retired;

        AddDomainEvent(new OperatingGeofenceRetiredEvent(
            OperatingGeofenceEventId.New(), OperatingGeofenceId, retiredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IOperatingGeofenceDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params OperatingGeofenceStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for OperatingGeofence {OperatingGeofenceId}.");
    }
}
