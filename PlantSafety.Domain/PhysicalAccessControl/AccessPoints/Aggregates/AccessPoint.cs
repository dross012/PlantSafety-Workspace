// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.AccessPoints
//  Layer:          Domain
//  File:           AccessPoint.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single physical access-control device (badge reader,
//                  turnstile, mantrap door, or vehicle gate). Tracks the device's lifecycle
//                  and, optionally, the SafetyZone it guards for occupancy correlation.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates;

/// <summary>Aggregate root for a single physical access-control device.</summary>
public sealed partial class AccessPoint
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this AccessPoint aggregate.</summary>
    public AccessPointId AccessPointId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public AccessPointStatus Status { get; private set; }

    // ---- Device identity -----------------------------------------------------

    /// <summary>Human-readable code for this access point (e.g. door/reader label).</summary>
    public string AccessPointCode { get; private set; } = string.Empty;

    /// <summary>Narrative description / physical location of this device.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Physical device classification.</summary>
    public AccessPointDeviceType DeviceType { get; private set; }

    /// <summary>Identity of the SafetyZone this access point guards, if any.</summary>
    public Guid? SafetyZoneId { get; private set; }

    /// <summary>Whether this access point is on the site perimeter (vs. an interior zone boundary).</summary>
    public bool IsPerimeter { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IAccessPointDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAccessPointDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAccessPointDomainEvent>(_domainEvents);

    // For rehydration only.
    private AccessPoint() { }

    // ---- Factory: Create (Register) -----------------------------------------------------------

    /// <summary>Registers a new physical access point and raises <see cref="AccessPointRegisteredEvent"/>.</summary>
    public static AccessPoint Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string accessPointCode = "",
        string description = "",
        AccessPointDeviceType deviceType = AccessPointDeviceType.BadgeReader,
        Guid? safetyZoneId = null,
        bool isPerimeter = false)
    {
        var agg = new AccessPoint
        {
            AccessPointId   = AccessPointId.New(),
            TenantId        = tenantId,
            SiteId          = siteId,
            Status          = AccessPointStatus.Registered,
            AccessPointCode = (accessPointCode ?? string.Empty).Trim(),
            Description     = (description ?? string.Empty).Trim(),
            DeviceType      = deviceType,
            SafetyZoneId    = safetyZoneId,
            IsPerimeter     = isPerimeter,
            CreatedAtUtc    = createdAtUtc,
            CreatedBy       = createdBy,
        };

        agg.AddDomainEvent(new AccessPointRegisteredEvent(
            AccessPointEventId.New(), agg.AccessPointId, deviceType, safetyZoneId, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: Activate -----------------------------------------------

    /// <summary>Places the access point into active service.</summary>
    public void Activate(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(AccessPointStatus.Registered, AccessPointStatus.Maintenance);

        Status = AccessPointStatus.Active;

        AddDomainEvent(new AccessPointActivatedEvent(
            AccessPointEventId.New(), AccessPointId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: ScheduleMaintenance -----------------------------------------------

    /// <summary>Takes the access point offline for maintenance.</summary>
    public void ScheduleMaintenance(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(AccessPointStatus.Active);

        Status = AccessPointStatus.Maintenance;

        AddDomainEvent(new AccessPointMaintenanceScheduledEvent(
            AccessPointEventId.New(), AccessPointId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Decommission -----------------------------------------------------------

    /// <summary>Permanently decommissions the access point.</summary>
    public void Decommission(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(AccessPointStatus.Registered, AccessPointStatus.Active, AccessPointStatus.Maintenance);

        Status = AccessPointStatus.Decommissioned;

        AddDomainEvent(new AccessPointDecommissionedEvent(
            AccessPointEventId.New(), AccessPointId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IAccessPointDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params AccessPointStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for AccessPoint {AccessPointId}.");
    }
}
