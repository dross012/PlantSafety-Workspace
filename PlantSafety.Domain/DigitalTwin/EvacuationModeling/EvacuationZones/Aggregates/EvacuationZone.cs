// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationZones
//  Layer:          Domain
//  File:           EvacuationZone.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single occupancy-tracked evacuation zone (Occupancy
//                  Tracking subdomain). Tracks rated capacity, an occupancy-threshold alerting
//                  rule, and an in-aggregate history of occupancy readings sourced from manual
//                  headcount, BMS feeds (Infrastructure.Integrations), sensor feeds, or egress
//                  simulation results. Occupancy data is aggregate/anonymized per module
//                  data-governance rules -- individual badge/sensor identities are never retained
//                  on the reading history. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationZones.Aggregates;

/// <summary>Aggregate root for a single occupancy-tracked evacuation zone.</summary>
public sealed partial class EvacuationZone
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this EvacuationZone aggregate.</summary>
    public EvacuationZoneId EvacuationZoneId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public EvacuationZoneStatus Status { get; private set; }

    // ---- Zone identity --------------------------------------------------------

    /// <summary>Human-readable name of this zone.</summary>
    public string ZoneName { get; private set; } = string.Empty;

    /// <summary>Version of the Digital Twin model this zone's geometry was sourced from.</summary>
    public string TwinModelVersion { get; private set; } = string.Empty;

    /// <summary>Rated maximum occupancy capacity of this zone.</summary>
    public int ZoneCapacity { get; private set; }

    /// <summary>Occupancy count above which <see cref="OccupancyThresholdExceededEvent"/> is raised.</summary>
    public int OccupancyThreshold { get; private set; }

    /// <summary>Most recently recorded occupancy headcount estimate.</summary>
    public int CurrentOccupancyCount { get; private set; }

    /// <summary>UTC timestamp of the most recent occupancy reading, if any.</summary>
    public UtcTimestamp? LastOccupancyReadingAtUtc { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Occupancy reading history ----------------------------------------------

    private readonly List<OccupancyReading> _occupancyReadings = [];

    /// <summary>Chronological, aggregate/anonymized history of occupancy readings for this zone.</summary>
    public IReadOnlyList<OccupancyReading> OccupancyReadings => _occupancyReadings.AsReadOnly();

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IEvacuationZoneDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IEvacuationZoneDomainEvent> DomainEvents
        => new ReadOnlyCollection<IEvacuationZoneDomainEvent>(_domainEvents);

    // For rehydration only.
    private EvacuationZone() { }

    // ---- Factory: Create (Define) -----------------------------------------------------------

    /// <summary>Defines a new evacuation zone against a validated Digital Twin model version and raises <see cref="EvacuationZoneDefinedEvent"/>.</summary>
    public static EvacuationZone Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string zoneName = "",
        string twinModelVersion = "",
        int zoneCapacity = 0,
        int occupancyThreshold = 0)
    {
        if (string.IsNullOrWhiteSpace(twinModelVersion))
            throw new ArgumentException("A validated Digital Twin model version is required to define a zone.", nameof(twinModelVersion));

        var agg = new EvacuationZone
        {
            EvacuationZoneId    = EvacuationZoneId.New(),
            TenantId            = tenantId,
            SiteId              = siteId,
            Status              = EvacuationZoneStatus.Active,
            ZoneName            = (zoneName ?? string.Empty).Trim(),
            TwinModelVersion    = twinModelVersion.Trim(),
            ZoneCapacity        = zoneCapacity,
            OccupancyThreshold  = occupancyThreshold,
            CreatedAtUtc        = createdAtUtc,
            CreatedBy           = createdBy,
        };

        agg.AddDomainEvent(new EvacuationZoneDefinedEvent(
            EvacuationZoneEventId.New(), agg.EvacuationZoneId, twinModelVersion, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: RecordOccupancySnapshot -----------------------------------------------

    /// <summary>
    /// Records a new occupancy snapshot (headcount, BMS feed, sensor feed, or simulation-derived
    /// reading). Always raises <see cref="OccupancySnapshotRecordedEvent"/>; additionally raises
    /// <see cref="OccupancyThresholdExceededEvent"/> and transitions to
    /// <see cref="EvacuationZoneStatus.OccupancyThresholdBreached"/> when the reading exceeds
    /// <see cref="OccupancyThreshold"/> (feeds Alerts.LifeSafetyEmergency via AlertRoutingService).
    /// </summary>
    public void RecordOccupancySnapshot(
        OccupancyReadingId readingId, int headcountEstimate, OccupancyReadingSource source,
        UtcTimestamp recordedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (headcountEstimate < 0)
            throw new ArgumentOutOfRangeException(nameof(headcountEstimate), "Headcount estimate must not be negative.");

        var exceededThreshold = OccupancyThreshold > 0 && headcountEstimate > OccupancyThreshold;

        _occupancyReadings.Add(new OccupancyReading(readingId, EvacuationZoneId, headcountEstimate, source, recordedAtUtc, exceededThreshold));

        CurrentOccupancyCount     = headcountEstimate;
        LastOccupancyReadingAtUtc = recordedAtUtc;
        Status                    = exceededThreshold ? EvacuationZoneStatus.OccupancyThresholdBreached : EvacuationZoneStatus.Active;

        AddDomainEvent(new OccupancySnapshotRecordedEvent(
            EvacuationZoneEventId.New(), EvacuationZoneId, readingId, headcountEstimate, recordedAtUtc, actor, correlationId, causationId));

        if (exceededThreshold)
        {
            AddDomainEvent(new OccupancyThresholdExceededEvent(
                EvacuationZoneEventId.New(), EvacuationZoneId, readingId, headcountEstimate, OccupancyThreshold, recordedAtUtc, actor, correlationId, causationId));
        }
    }

    // ---- Behavior: Retire -----------------------------------------------------------

    /// <summary>Retires this evacuation zone.</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == EvacuationZoneStatus.Retired)
            throw new EvacuationZoneInvalidStatusTransitionException(EvacuationZoneId, Status.ToString(), EvacuationZoneStatus.Retired.ToString(), correlationId, causationId);

        Status = EvacuationZoneStatus.Retired;

        AddDomainEvent(new EvacuationZoneRetiredEvent(
            EvacuationZoneEventId.New(), EvacuationZoneId, retiredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IEvacuationZoneDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
