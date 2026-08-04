// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportTrips
//  Layer:          Domain
//  File:           TransportTrip.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single planned material/personnel transport trip. Models the
//                  real lifecycle: planning, completion (with actual departure/arrival and distance),
//                  and cancellation.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportTrips.Aggregates;

/// <summary>Aggregate root for a single planned transport trip.</summary>
public sealed partial class TransportTrip
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this TransportTrip aggregate.</summary>
    public TransportTripId TransportTripId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public TransportTripStatus Status { get; private set; }

    // ---- Trip identity ---------------------------------------------------

    /// <summary>Identity of the vehicle assigned to this trip.</summary>
    public Guid VehicleId { get; private set; }

    /// <summary>Identity of the driver assigned to this trip.</summary>
    public Guid DriverUserId { get; private set; }

    /// <summary>Trip purpose classification.</summary>
    public TransportTripType TripType { get; private set; }

    /// <summary>Route description for this trip.</summary>
    public string RouteDescription { get; private set; } = string.Empty;

    /// <summary>Planned UTC departure time.</summary>
    public UtcTimestamp PlannedDepartureAtUtc { get; private set; }

    /// <summary>Planned UTC arrival time.</summary>
    public UtcTimestamp PlannedArrivalAtUtc { get; private set; }

    /// <summary>Actual UTC departure time, once completed.</summary>
    public UtcTimestamp? ActualDepartureAtUtc { get; private set; }

    /// <summary>Actual UTC arrival time, once completed.</summary>
    public UtcTimestamp? ActualArrivalAtUtc { get; private set; }

    /// <summary>Distance travelled in miles, once completed.</summary>
    public decimal? DistanceMiles { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ITransportTripDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ITransportTripDomainEvent> DomainEvents
        => new ReadOnlyCollection<ITransportTripDomainEvent>(_domainEvents);

    // For rehydration only.
    private TransportTrip() { }

    // ---- Factory: Create (Plan) -----------------------------------------------------------

    /// <summary>Plans a new transport trip and raises <see cref="TransportTripPlannedEvent"/>.</summary>
    public static TransportTrip Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid vehicleId = default,
        Guid driverUserId = default,
        TransportTripType tripType = TransportTripType.OnSiteMaterialMove,
        string routeDescription = "",
        UtcTimestamp plannedDepartureAtUtc = default,
        UtcTimestamp plannedArrivalAtUtc = default)
    {
        var agg = new TransportTrip
        {
            TransportTripId         = TransportTripId.New(),
            TenantId                 = tenantId,
            SiteId                   = siteId,
            Status                   = TransportTripStatus.Planned,
            VehicleId                = vehicleId,
            DriverUserId             = driverUserId,
            TripType                 = tripType,
            RouteDescription         = (routeDescription ?? string.Empty).Trim(),
            PlannedDepartureAtUtc    = plannedDepartureAtUtc,
            PlannedArrivalAtUtc      = plannedArrivalAtUtc,
            CreatedAtUtc             = createdAtUtc,
            CreatedBy                = createdBy,
        };

        agg.AddDomainEvent(new TransportTripPlannedEvent(
            TransportTripEventId.New(), agg.TransportTripId, tripType, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: Complete -----------------------------------------

    /// <summary>Completes this trip, recording actual departure/arrival and distance travelled.</summary>
    public void Complete(UtcTimestamp actualDepartureAtUtc, UtcTimestamp actualArrivalAtUtc, decimal distanceMiles, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(TransportTripStatus.Planned, TransportTripStatus.InProgress);

        if (distanceMiles < 0)
            throw new ArgumentOutOfRangeException(nameof(distanceMiles), "Distance travelled cannot be negative.");

        ActualDepartureAtUtc = actualDepartureAtUtc;
        ActualArrivalAtUtc   = actualArrivalAtUtc;
        DistanceMiles        = distanceMiles;
        Status = TransportTripStatus.Completed;

        AddDomainEvent(new TransportTripCompletedEvent(
            TransportTripEventId.New(), TransportTripId, distanceMiles, actualArrivalAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Cancel -----------------------------------------------------------

    /// <summary>Cancels this trip before completion.</summary>
    public void Cancel(UtcTimestamp cancelledAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(TransportTripStatus.Planned, TransportTripStatus.InProgress);

        Status = TransportTripStatus.Cancelled;

        AddDomainEvent(new TransportTripCancelledEvent(
            TransportTripEventId.New(), TransportTripId, cancelledAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ITransportTripDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params TransportTripStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new TransportTripInvalidStatusTransitionException(TransportTripId, Status.ToString(), "Requested", CorrelationId.New(), CausationId.New());
    }
}
