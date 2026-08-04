// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         LogisticsTransportSafety.TransportIncidents
//  Layer:          Domain
//  File:           TransportIncident.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single transport incident (collision, cargo spill, near-miss,
//                  rollover, pedestrian interaction). Models the real lifecycle: recording, investigation,
//                  and closure. Feeds RiskManagement (severity) and integrates with AccidentManagement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.LogisticsTransportSafety.TransportIncidents.Aggregates;

/// <summary>Aggregate root for a single transport incident.</summary>
public sealed partial class TransportIncident
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this TransportIncident aggregate.</summary>
    public TransportIncidentId TransportIncidentId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public TransportIncidentStatus Status { get; private set; }

    // ---- Incident identity ---------------------------------------------------

    /// <summary>Identity of the transport trip this incident occurred during, if any.</summary>
    public Guid? TripId { get; private set; }

    /// <summary>Identity of the vehicle involved in this incident.</summary>
    public Guid VehicleId { get; private set; }

    /// <summary>Identity of the driver involved in this incident.</summary>
    public Guid DriverUserId { get; private set; }

    /// <summary>Incident type classification.</summary>
    public TransportIncidentType IncidentType { get; private set; }

    /// <summary>Incident severity classification.</summary>
    public TransportIncidentSeverity Severity { get; private set; }

    /// <summary>UTC timestamp the incident occurred.</summary>
    public UtcTimestamp OccurredAtUtc { get; private set; }

    /// <summary>Location description where the incident occurred.</summary>
    public string Location { get; private set; } = string.Empty;

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ITransportIncidentDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ITransportIncidentDomainEvent> DomainEvents
        => new ReadOnlyCollection<ITransportIncidentDomainEvent>(_domainEvents);

    // For rehydration only.
    private TransportIncident() { }

    // ---- Factory: Create (Record) -----------------------------------------------------------

    /// <summary>Records a new transport incident and raises <see cref="TransportIncidentRecordedEvent"/>.</summary>
    public static TransportIncident Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid? tripId = null,
        Guid vehicleId = default,
        Guid driverUserId = default,
        TransportIncidentType incidentType = TransportIncidentType.NearMiss,
        TransportIncidentSeverity severity = TransportIncidentSeverity.Minor,
        string location = "")
    {
        var agg = new TransportIncident
        {
            TransportIncidentId = TransportIncidentId.New(),
            TenantId              = tenantId,
            SiteId                = siteId,
            Status                = TransportIncidentStatus.Reported,
            TripId                = tripId,
            VehicleId             = vehicleId,
            DriverUserId          = driverUserId,
            IncidentType          = incidentType,
            Severity              = severity,
            OccurredAtUtc         = createdAtUtc,
            Location              = (location ?? string.Empty).Trim(),
            CreatedAtUtc          = createdAtUtc,
            CreatedBy             = createdBy,
        };

        agg.AddDomainEvent(new TransportIncidentRecordedEvent(
            TransportIncidentEventId.New(), agg.TransportIncidentId, incidentType, severity, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: BeginInvestigation -----------------------------------------

    /// <summary>Begins investigation of this incident.</summary>
    public void BeginInvestigation(UtcTimestamp startedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(TransportIncidentStatus.Reported);

        Status = TransportIncidentStatus.UnderInvestigation;

        AddDomainEvent(new TransportIncidentInvestigationStartedEvent(
            TransportIncidentEventId.New(), TransportIncidentId, startedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Close -----------------------------------------------------------

    /// <summary>Closes this incident's investigation.</summary>
    public void Close(UtcTimestamp closedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(TransportIncidentStatus.UnderInvestigation);

        Status = TransportIncidentStatus.Closed;

        AddDomainEvent(new TransportIncidentClosedEvent(
            TransportIncidentEventId.New(), TransportIncidentId, closedAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ITransportIncidentDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params TransportIncidentStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new TransportIncidentInvalidStatusTransitionException(TransportIncidentId, Status.ToString(), "Requested", CorrelationId.New(), CausationId.New());
    }
}
