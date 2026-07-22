// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.EnvironmentalAssetMonitoring
//  Layer:          Domain
//  File:           EnvironmentalAssetMonitoring.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the EnvironmentalAssetMonitoring bounded context (module: Observability).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Aggregates;

/// <summary>Aggregate root for the EnvironmentalAssetMonitoring bounded context.</summary>
public sealed partial class EnvironmentalAssetMonitoring
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this EnvironmentalAssetMonitoring aggregate.</summary>
    public EnvironmentalAssetMonitoringId EnvironmentalAssetMonitoringId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public EnvironmentalAssetMonitoringStatus Status { get; private set; }

    // ---- EnvironmentalAssetMonitoring-specific business properties --------------------------------

    /// <summary>SensorId business attribute.</summary>
    public string SensorId { get; private set; }

    /// <summary>MeasurementType business attribute.</summary>
    public string MeasurementType { get; private set; }

    /// <summary>Value business attribute.</summary>
    public decimal Value { get; private set; }

    /// <summary>ThresholdExceeded business attribute.</summary>
    public bool ThresholdExceeded { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IEnvironmentalAssetMonitoringDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IEnvironmentalAssetMonitoringDomainEvent> DomainEvents
        => new ReadOnlyCollection<IEnvironmentalAssetMonitoringDomainEvent>(_domainEvents);

    // For rehydration only.
    private EnvironmentalAssetMonitoring() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new EnvironmentalAssetMonitoring aggregate and raises <see cref="EnvironmentalAssetMonitoringCreatedEvent"/>.</summary>
    public static EnvironmentalAssetMonitoring Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string sensorId = "",
        string measurementType = "",
        decimal value = 0m,
        bool thresholdExceeded = false)
    {
        var agg = new EnvironmentalAssetMonitoring
        {
            EnvironmentalAssetMonitoringId     = EnvironmentalAssetMonitoringId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = EnvironmentalAssetMonitoringStatus.Created,
            SensorId = (sensorId ?? string.Empty).Trim(),
            MeasurementType = (measurementType ?? string.Empty).Trim(),
            Value = value,
            ThresholdExceeded = thresholdExceeded,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new EnvironmentalAssetMonitoringCreatedEvent(
            EventId:       new EnvironmentalAssetMonitoringEventId(Uuid7.New()),
            EnvironmentalAssetMonitoringId:      agg.EnvironmentalAssetMonitoringId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IEnvironmentalAssetMonitoringDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params EnvironmentalAssetMonitoringStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for EnvironmentalAssetMonitoring {EnvironmentalAssetMonitoringId}.");
    }
}