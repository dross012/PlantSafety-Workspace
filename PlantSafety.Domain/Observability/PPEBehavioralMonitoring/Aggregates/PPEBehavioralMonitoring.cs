// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.PPEBehavioralMonitoring
//  Layer:          Domain
//  File:           PPEBehavioralMonitoring.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the PPEBehavioralMonitoring bounded context (module: Observability).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.PPEBehavioralMonitoring.Aggregates;

/// <summary>Aggregate root for the PPEBehavioralMonitoring bounded context.</summary>
public sealed partial class PPEBehavioralMonitoring
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this PPEBehavioralMonitoring aggregate.</summary>
    public PPEBehavioralMonitoringId PPEBehavioralMonitoringId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PPEBehavioralMonitoringStatus Status { get; private set; }

    // ---- PPEBehavioralMonitoring-specific business properties --------------------------------

    /// <summary>SensorId business attribute.</summary>
    public string SensorId { get; private set; }

    /// <summary>ZoneCode business attribute.</summary>
    public string ZoneCode { get; private set; }

    /// <summary>DetectedEventType business attribute.</summary>
    public string DetectedEventType { get; private set; }

    /// <summary>ConfidenceScore business attribute.</summary>
    public decimal ConfidenceScore { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPPEBehavioralMonitoringDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPPEBehavioralMonitoringDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPPEBehavioralMonitoringDomainEvent>(_domainEvents);

    // For rehydration only.
    private PPEBehavioralMonitoring() 
    {
        // Assigned by the data mapper immediately after construction; see the public
        // constructor for the invariant these properties actually hold.
        DetectedEventType = null!;
        SensorId = null!;
        ZoneCode = null!;
    }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new PPEBehavioralMonitoring aggregate and raises <see cref="PPEBehavioralMonitoringCreatedEvent"/>.</summary>
    public static PPEBehavioralMonitoring Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string sensorId = "",
        string zoneCode = "",
        string detectedEventType = "",
        decimal confidenceScore = 0m)
    {
        var agg = new PPEBehavioralMonitoring
        {
            PPEBehavioralMonitoringId     = PPEBehavioralMonitoringId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = PPEBehavioralMonitoringStatus.Created,
            SensorId = (sensorId ?? string.Empty).Trim(),
            ZoneCode = (zoneCode ?? string.Empty).Trim(),
            DetectedEventType = (detectedEventType ?? string.Empty).Trim(),
            ConfidenceScore = confidenceScore,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new PPEBehavioralMonitoringCreatedEvent(
            EventId:       new PPEBehavioralMonitoringEventId(Uuid7.New()),
            PPEBehavioralMonitoringId:      agg.PPEBehavioralMonitoringId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPPEBehavioralMonitoringDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PPEBehavioralMonitoringStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for PPEBehavioralMonitoring {PPEBehavioralMonitoringId}.");
    }
}