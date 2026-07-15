// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Domain
//  File:           EnvironmentalHealth.cs
//  Created:        2026-06-17
//  Updated:        2026-07-15 — extended from identity-only scaffold with the full
//                  environmental monitoring record shape (site, metric, measurement,
//                  threshold, status) to back the Application/Api CQRS slice.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the EnvironmentalHealth module. Tracks a single
//                  environmental monitoring reading (air quality, noise, water quality,
//                  emissions, waste handling, etc.) captured at a site.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.Aggregates;

/// <summary>Aggregate root for the EnvironmentalHealth bounded context.</summary>
public sealed class EnvironmentalHealth
{
    /// <summary>Unique identity of this EnvironmentalHealth aggregate.</summary>
    public EnvironmentalHealthId EnvironmentalHealthId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this reading was captured at. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Environmental metric type (e.g., AirQuality, NoiseLevel, WaterQuality, Emissions, WasteHandling).</summary>
    public string MetricType { get; private set; } = string.Empty;

    /// <summary>Measured value for the metric.</summary>
    public decimal MeasurementValue { get; private set; }

    /// <summary>Unit of measure for <see cref="MeasurementValue"/> (e.g., ppm, dB, AQI).</summary>
    public string Unit { get; private set; } = string.Empty;

    /// <summary>Regulatory/internal threshold this measurement is evaluated against.</summary>
    public decimal Threshold { get; private set; }

    /// <summary>Evaluated status: Normal, Warning, Critical.</summary>
    public string Status { get; private set; } = "Normal";

    /// <summary>UTC timestamp the reading was recorded.</summary>
    public UtcTimestamp RecordedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // Domain events
    private readonly List<IEnvironmentalHealthDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IEnvironmentalHealthDomainEvent> DomainEvents
        => new ReadOnlyCollection<IEnvironmentalHealthDomainEvent>(_domainEvents);

    private EnvironmentalHealth() { }

    /// <summary>Creates a new EnvironmentalHealth aggregate and raises <see cref="EnvironmentalHealthCreatedEvent"/>.</summary>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="siteId">Optional owning site.</param>
    /// <param name="metricType">Environmental metric type. Must not be null or whitespace.</param>
    /// <param name="measurementValue">Measured value.</param>
    /// <param name="unit">Unit of measure. Must not be null or whitespace.</param>
    /// <param name="threshold">Threshold the measurement is evaluated against.</param>
    /// <param name="status">Evaluated status (Normal/Warning/Critical). Must not be null or whitespace.</param>
    /// <param name="createdBy">CRUD authorship identity.</param>
    /// <param name="createdAtUtc">Creation timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="metricType"/>, <paramref name="unit"/>, or <paramref name="status"/> is null or whitespace.</exception>
    public static EnvironmentalHealth Create(
        Guid tenantId, Guid? siteId, string metricType, decimal measurementValue, string unit,
        decimal threshold, string status,
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor)
    {
        if (string.IsNullOrWhiteSpace(metricType))
            throw new ArgumentException("MetricType must not be null or whitespace.", nameof(metricType));
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Unit must not be null or whitespace.", nameof(unit));
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status must not be null or whitespace.", nameof(status));

        var agg = new EnvironmentalHealth
        {
            EnvironmentalHealthId = EnvironmentalHealthId.New(),
            TenantId              = tenantId,
            SiteId                = siteId,
            MetricType            = metricType.Trim(),
            MeasurementValue      = measurementValue,
            Unit                  = unit.Trim(),
            Threshold             = threshold,
            Status                = status.Trim(),
            RecordedAtUtc         = createdAtUtc,
            CreatedAtUtc          = createdAtUtc,
            CreatedBy             = createdBy,
        };

        agg._domainEvents.Add(new EnvironmentalHealthCreatedEvent(
            EventId:                Uuid7.New(),
            EnvironmentalHealthId:  agg.EnvironmentalHealthId.Value,
            TenantId:               tenantId,
            MetricType:             agg.MetricType,
            MeasurementValue:       measurementValue,
            Status:                 agg.Status,
            CreatedAtUtc:           createdAtUtc,
            Actor:                  actor,
            CorrelationId:          correlationId,
            CausationId:            causationId));

        return agg;
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
