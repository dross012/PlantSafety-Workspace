// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.SafetyZones
//  Layer:          Domain
//  File:           SafetyZone.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single physical safety zone whose access and
//                  occupancy are correlated from ingested AccessEvent data (Zone Occupancy
//                  Correlation subdomain).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates;

/// <summary>Aggregate root for a single physical safety zone.</summary>
public sealed partial class SafetyZone
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this SafetyZone aggregate.</summary>
    public SafetyZoneId SafetyZoneId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public SafetyZoneStatus Status { get; private set; }

    // ---- Zone identity -----------------------------------------------------

    /// <summary>Human-readable code for this zone.</summary>
    public string ZoneCode { get; private set; } = string.Empty;

    /// <summary>Human-readable name of this zone.</summary>
    public string ZoneName { get; private set; } = string.Empty;

    /// <summary>Hazard classification for this zone.</summary>
    public SafetyZoneHazardLevel HazardLevel { get; private set; }

    /// <summary>Maximum permitted concurrent occupancy.</summary>
    public int MaxOccupancy { get; private set; }

    /// <summary>Current occupancy, as correlated from ingested AccessEvents.</summary>
    public int CurrentOccupancy { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ISafetyZoneDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ISafetyZoneDomainEvent> DomainEvents
        => new ReadOnlyCollection<ISafetyZoneDomainEvent>(_domainEvents);

    // For rehydration only.
    private SafetyZone() { }

    // ---- Factory: Create (Register) -----------------------------------------------------------

    /// <summary>Registers a new safety zone and raises <see cref="SafetyZoneRegisteredEvent"/>.</summary>
    public static SafetyZone Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string zoneCode = "",
        string zoneName = "",
        SafetyZoneHazardLevel hazardLevel = SafetyZoneHazardLevel.Low,
        int maxOccupancy = 0)
    {
        var agg = new SafetyZone
        {
            SafetyZoneId     = SafetyZoneId.New(),
            TenantId         = tenantId,
            SiteId           = siteId,
            Status           = SafetyZoneStatus.Active,
            ZoneCode         = (zoneCode ?? string.Empty).Trim(),
            ZoneName         = (zoneName ?? string.Empty).Trim(),
            HazardLevel      = hazardLevel,
            MaxOccupancy     = maxOccupancy,
            CurrentOccupancy = 0,
            CreatedAtUtc     = createdAtUtc,
            CreatedBy        = createdBy,
        };

        agg.AddDomainEvent(new SafetyZoneRegisteredEvent(
            SafetyZoneEventId.New(), agg.SafetyZoneId, hazardLevel, maxOccupancy, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: Restrict -----------------------------------------------

    /// <summary>Places the zone under a restricted-access policy.</summary>
    public void Restrict(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(SafetyZoneStatus.Active);

        Status = SafetyZoneStatus.Restricted;

        AddDomainEvent(new SafetyZoneRestrictedEvent(
            SafetyZoneEventId.New(), SafetyZoneId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Deactivate -----------------------------------------------------------

    /// <summary>Deactivates the zone; occupancy is no longer tracked.</summary>
    public void Deactivate(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(SafetyZoneStatus.Active, SafetyZoneStatus.Restricted);

        Status = SafetyZoneStatus.Inactive;

        AddDomainEvent(new SafetyZoneDeactivatedEvent(
            SafetyZoneEventId.New(), SafetyZoneId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: UpdateOccupancy -----------------------------------------------------------

    /// <summary>Recalculates current occupancy from ingested AccessEvent In/Out correlation.</summary>
    public void UpdateOccupancy(int currentOccupancy, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (currentOccupancy < 0)
            throw new ArgumentOutOfRangeException(nameof(currentOccupancy), "Occupancy cannot be negative.");

        CurrentOccupancy = currentOccupancy;

        AddDomainEvent(new SafetyZoneOccupancyUpdatedEvent(
            SafetyZoneEventId.New(), SafetyZoneId, currentOccupancy, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ISafetyZoneDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params SafetyZoneStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for SafetyZone {SafetyZoneId}.");
    }
}
