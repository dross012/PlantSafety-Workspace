// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.HumanRobotInteractionEvent
//  Layer:          Domain
//  File:           HumanRobotInteractionEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single human-robot interaction event (proximity alert, near
//                  miss, physical contact, or emergency-stop trigger) between a worker and an
//                  autonomous vehicle/mobile-robot asset. Feeds the mean-distance-between-near-misses
//                  KPI and RiskManagement anomaly hooks. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.HumanRobotInteractionEvent.Aggregates;

/// <summary>Aggregate root for a single human-robot interaction event.</summary>
public sealed partial class HumanRobotInteractionEvent
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this HumanRobotInteractionEvent aggregate.</summary>
    public HumanRobotInteractionEventId HumanRobotInteractionEventId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public HumanRobotInteractionEventStatus Status { get; private set; }

    // ---- Interaction detail ----------------------------------------------------

    /// <summary>Identity of the autonomous vehicle asset involved in the interaction.</summary>
    public Guid AutonomousVehicleAssetId { get; private set; }

    /// <summary>Interaction classification.</summary>
    public HumanRobotInteractionType InteractionType { get; private set; }

    /// <summary>Severity classification.</summary>
    public InteractionSeverityLevel SeverityLevel { get; private set; }

    /// <summary>Measured separation distance in meters, if available.</summary>
    public decimal? DistanceMeters { get; private set; }

    /// <summary>Free-text description of the interaction location.</summary>
    public string LocationDescription { get; private set; } = string.Empty;

    /// <summary>UTC timestamp the interaction was detected.</summary>
    public UtcTimestamp DetectedAtUtc { get; private set; }

    /// <summary>Identity of the user who reviewed the event, if reviewed.</summary>
    public UserId? ReviewedByUserId { get; private set; }

    /// <summary>UTC timestamp the event was reviewed, if reviewed.</summary>
    public UtcTimestamp? ReviewedAtUtc { get; private set; }

    // ---- Audit -------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -------------------------------------------------------

    private readonly List<IHumanRobotInteractionEventDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IHumanRobotInteractionEventDomainEvent> DomainEvents
        => new ReadOnlyCollection<IHumanRobotInteractionEventDomainEvent>(_domainEvents);

    // For rehydration only.
    private HumanRobotInteractionEvent() { }

    // ---- Factory: Create (Record) -----------------------------------------------

    /// <summary>Records a new human-robot interaction event and raises <see cref="HumanRobotInteractionEventRecordedEvent"/>.</summary>
    public static HumanRobotInteractionEvent Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid autonomousVehicleAssetId = default,
        HumanRobotInteractionType interactionType = HumanRobotInteractionType.ProximityAlert,
        InteractionSeverityLevel severityLevel = InteractionSeverityLevel.Low,
        decimal? distanceMeters = null,
        string locationDescription = "")
    {
        var agg = new HumanRobotInteractionEvent
        {
            HumanRobotInteractionEventId = HumanRobotInteractionEventId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = HumanRobotInteractionEventStatus.Recorded,
            AutonomousVehicleAssetId = autonomousVehicleAssetId,
            InteractionType = interactionType,
            SeverityLevel = severityLevel,
            DistanceMeters = distanceMeters,
            LocationDescription = (locationDescription ?? string.Empty).Trim(),
            DetectedAtUtc = createdAtUtc,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new HumanRobotInteractionEventRecordedEvent(
            HumanRobotInteractionEventEventId.New(), agg.HumanRobotInteractionEventId, autonomousVehicleAssetId, interactionType, severityLevel, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: BeginReview --------------------------------------------------

    /// <summary>Begins safety review of the interaction event.</summary>
    public void BeginReview(PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(HumanRobotInteractionEventStatus.Recorded);

        Status = HumanRobotInteractionEventStatus.UnderReview;

        AddDomainEvent(new HumanRobotInteractionEventReviewStartedEvent(
            HumanRobotInteractionEventEventId.New(), HumanRobotInteractionEventId, DetectedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Close ---------------------------------------------------------

    /// <summary>Closes the interaction event review.</summary>
    public void Close(UserId reviewedByUserId, UtcTimestamp reviewedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(HumanRobotInteractionEventStatus.Recorded, HumanRobotInteractionEventStatus.UnderReview);

        ReviewedByUserId = reviewedByUserId;
        ReviewedAtUtc = reviewedAtUtc;
        Status = HumanRobotInteractionEventStatus.Closed;

        AddDomainEvent(new HumanRobotInteractionEventClosedEvent(
            HumanRobotInteractionEventEventId.New(), HumanRobotInteractionEventId, reviewedAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IHumanRobotInteractionEventDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params HumanRobotInteractionEventStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for HumanRobotInteractionEvent {HumanRobotInteractionEventId}.");
    }
}
