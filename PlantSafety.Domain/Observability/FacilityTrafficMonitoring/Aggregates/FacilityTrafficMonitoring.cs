// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.FacilityTrafficMonitoring
//  Layer:          Domain
//  File:           FacilityTrafficMonitoring.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the FacilityTrafficMonitoring bounded context (module: Observability).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.FacilityTrafficMonitoring.Aggregates;

/// <summary>Aggregate root for the FacilityTrafficMonitoring bounded context.</summary>
public sealed partial class FacilityTrafficMonitoring
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this FacilityTrafficMonitoring aggregate.</summary>
    public FacilityTrafficMonitoringId FacilityTrafficMonitoringId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public FacilityTrafficMonitoringStatus Status { get; private set; }

    // ---- FacilityTrafficMonitoring-specific business properties --------------------------------

    /// <summary>ZoneCode business attribute.</summary>
    public string ZoneCode { get; private set; }

    /// <summary>EventType business attribute.</summary>
    public string EventType { get; private set; }

    /// <summary>SeverityScore business attribute.</summary>
    public decimal SeverityScore { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IFacilityTrafficMonitoringDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IFacilityTrafficMonitoringDomainEvent> DomainEvents
        => new ReadOnlyCollection<IFacilityTrafficMonitoringDomainEvent>(_domainEvents);

    // For rehydration only.
    private FacilityTrafficMonitoring() 
    {
        // Assigned by the data mapper immediately after construction; see the public
        // constructor for the invariant these properties actually hold.
        EventType = null!;
        ZoneCode = null!;
    }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new FacilityTrafficMonitoring aggregate and raises <see cref="FacilityTrafficMonitoringCreatedEvent"/>.</summary>
    public static FacilityTrafficMonitoring Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string zoneCode = "",
        string eventType = "",
        decimal severityScore = 0m)
    {
        var agg = new FacilityTrafficMonitoring
        {
            FacilityTrafficMonitoringId     = FacilityTrafficMonitoringId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = FacilityTrafficMonitoringStatus.Created,
            ZoneCode = (zoneCode ?? string.Empty).Trim(),
            EventType = (eventType ?? string.Empty).Trim(),
            SeverityScore = severityScore,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new FacilityTrafficMonitoringCreatedEvent(
            EventId:       new FacilityTrafficMonitoringEventId(Uuid7.New()),
            FacilityTrafficMonitoringId:      agg.FacilityTrafficMonitoringId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IFacilityTrafficMonitoringDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params FacilityTrafficMonitoringStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for FacilityTrafficMonitoring {FacilityTrafficMonitoringId}.");
    }
}