// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Domain
//  File:           EnvironmentalHealth.cs
//  Created:        2026-06-17
//  Updated:        2026-07-15 — extended from identity-only scaffold with the full
//                  environmental monitoring record shape (site, metric, measurement,
//                  threshold, status) to back the Application/Api CQRS slice.
//  Updated:        2026-08-02 — Phase 4 full build-out: renamed MetricType -> MonitoringType,
//                  MeasurementValue -> MeasuredValue (decimal?), and the free-text business
//                  Status ("Normal"/"Warning"/"Critical") -> EvaluatedStatus, freeing the
//                  "Status" name for the new standard lifecycle enum (EnvironmentalHealthStatus)
//                  shared by every other module in this build-out. Added ThresholdExceeded
//                  (bool) plus the full Created->Active->Completed->Closed->Archived lifecycle.
//                  No external consumers referenced the renamed members prior to this change
//                  (verified by search — this aggregate had no Application/Infrastructure/Api
//                  layer built on it yet). Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing's build-out shape.
//                  Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the EnvironmentalHealth module. Tracks a single
//                  environmental-health monitoring record (air/water/noise/soil) captured at a
//                  site. Distinct bounded context from the live
//                  Features.EnvironmentalHealth.Handlers.IEnvironmentalHealthReadingRepository
//                  "reading" vertical and from EmployeeSafety.EnvironmentalHealthMonitoring.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.Aggregates;

/// <summary>Aggregate root for the EnvironmentalHealth bounded context.</summary>
public sealed class EnvironmentalHealth
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Unique identity of this EnvironmentalHealth aggregate.</summary>
    public EnvironmentalHealthId EnvironmentalHealthId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this reading was captured at. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public EnvironmentalHealthStatus Status { get; private set; }

    // ---- EnvironmentalHealth-specific business properties -----------------------

    /// <summary>Environmental monitoring classification (e.g. AirQuality, WaterQuality, NoiseLevel, SoilContamination).</summary>
    public string MonitoringType { get; private set; } = string.Empty;

    /// <summary>Measured value for the monitored metric, when captured.</summary>
    public decimal? MeasuredValue { get; private set; }

    /// <summary>Unit of measure for <see cref="MeasuredValue"/> (e.g. ppm, dB, AQI).</summary>
    public string Unit { get; private set; } = string.Empty;

    /// <summary>Regulatory/internal threshold this measurement is evaluated against.</summary>
    public decimal Threshold { get; private set; }

    /// <summary>Whether the measured value exceeded <see cref="Threshold"/>.</summary>
    public bool ThresholdExceeded { get; private set; }

    /// <summary>Evaluated business classification: Normal, Warning, Critical.</summary>
    public string EvaluatedStatus { get; private set; } = "Normal";

    /// <summary>UTC timestamp the reading was recorded.</summary>
    public UtcTimestamp RecordedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IEnvironmentalHealthDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IEnvironmentalHealthDomainEvent> DomainEvents
        => new ReadOnlyCollection<IEnvironmentalHealthDomainEvent>(_domainEvents);

    private EnvironmentalHealth() { }

    /// <summary>Creates a new EnvironmentalHealth aggregate and raises <see cref="EnvironmentalHealthCreatedEvent"/>.</summary>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="siteId">Optional owning site.</param>
    /// <param name="monitoringType">Environmental monitoring classification. Must not be null or whitespace.</param>
    /// <param name="measuredValue">Measured value, if captured.</param>
    /// <param name="unit">Unit of measure. Must not be null or whitespace.</param>
    /// <param name="threshold">Threshold the measurement is evaluated against.</param>
    /// <param name="thresholdExceeded">Whether the measured value exceeded <paramref name="threshold"/>.</param>
    /// <param name="evaluatedStatus">Evaluated business classification (Normal/Warning/Critical). Must not be null or whitespace.</param>
    /// <param name="createdBy">CRUD authorship identity.</param>
    /// <param name="createdAtUtc">Creation timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="monitoringType"/>, <paramref name="unit"/>, or <paramref name="evaluatedStatus"/> is null or whitespace.</exception>
    public static EnvironmentalHealth Create(
        Guid tenantId, Guid? siteId, string monitoringType, decimal? measuredValue, string unit,
        decimal threshold, bool thresholdExceeded, string evaluatedStatus,
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor)
    {
        if (string.IsNullOrWhiteSpace(monitoringType))
            throw new ArgumentException("MonitoringType must not be null or whitespace.", nameof(monitoringType));
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Unit must not be null or whitespace.", nameof(unit));
        if (string.IsNullOrWhiteSpace(evaluatedStatus))
            throw new ArgumentException("EvaluatedStatus must not be null or whitespace.", nameof(evaluatedStatus));

        var agg = new EnvironmentalHealth
        {
            EnvironmentalHealthId = EnvironmentalHealthId.New(),
            TenantId              = tenantId,
            SiteId                = siteId,
            Status                = EnvironmentalHealthStatus.Created,
            MonitoringType        = monitoringType.Trim(),
            MeasuredValue         = measuredValue,
            Unit                  = unit.Trim(),
            Threshold             = threshold,
            ThresholdExceeded     = thresholdExceeded,
            EvaluatedStatus       = evaluatedStatus.Trim(),
            RecordedAtUtc         = createdAtUtc,
            CreatedAtUtc          = createdAtUtc,
            CreatedBy             = createdBy,
        };

        agg._domainEvents.Add(new EnvironmentalHealthCreatedEvent(
            EventId:                Uuid7.New(),
            EnvironmentalHealthId:  agg.EnvironmentalHealthId.Value,
            TenantId:               tenantId,
            MonitoringType:         agg.MonitoringType,
            MeasuredValue:          measuredValue,
            EvaluatedStatus:        agg.EvaluatedStatus,
            CreatedAtUtc:           createdAtUtc,
            Actor:                  actor,
            CorrelationId:          correlationId,
            CausationId:            causationId));

        return agg;
    }

    // ---- Lifecycle: Activate / Complete / Close / Archive -----------------------
    // Phase 4 full build-out (2026-08-02): this aggregate previously had only the Create
    // factory (no lifecycle transitions at all). Mirrors
    // PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing's build-out shape, using the
    // standard Activate/Complete/Close/Archive verbs.

    /// <summary>Activates the environmental-health record (Created -&gt; Active).</summary>
    public void Activate(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EnvironmentalHealthLifecycleRules.CanActivate, EnvironmentalHealthStatus.Created);

        Status = EnvironmentalHealthStatus.Active;

        AddDomainEvent(new EnvironmentalHealthActivatedEvent(
            EventId: Uuid7.New(),
            EnvironmentalHealthId: EnvironmentalHealthId.Value,
            ActorUserId: actorUserId,
            ActivatedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Completes the environmental-health record (Active -&gt; Completed).</summary>
    public void Complete(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EnvironmentalHealthLifecycleRules.CanComplete, EnvironmentalHealthStatus.Active);

        Status = EnvironmentalHealthStatus.Completed;

        AddDomainEvent(new EnvironmentalHealthCompletedEvent(
            EventId: Uuid7.New(),
            EnvironmentalHealthId: EnvironmentalHealthId.Value,
            ActorUserId: actorUserId,
            CompletedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Closes the environmental-health record (Completed -&gt; Closed).</summary>
    public void Close(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EnvironmentalHealthLifecycleRules.CanClose, EnvironmentalHealthStatus.Completed);

        Status = EnvironmentalHealthStatus.Closed;

        AddDomainEvent(new EnvironmentalHealthClosedEvent(
            EventId: Uuid7.New(),
            EnvironmentalHealthId: EnvironmentalHealthId.Value,
            ActorUserId: actorUserId,
            ClosedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Archives the environmental-health record for long-term retention (Closed -&gt; Archived).</summary>
    public void Archive(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EnvironmentalHealthLifecycleRules.CanArchive, EnvironmentalHealthStatus.Closed);

        Status = EnvironmentalHealthStatus.Archived;

        AddDomainEvent(new EnvironmentalHealthArchivedEvent(
            EventId: Uuid7.New(),
            EnvironmentalHealthId: EnvironmentalHealthId.Value,
            ActorUserId: actorUserId,
            ArchivedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void AddDomainEvent(IEnvironmentalHealthDomainEvent evt) => _domainEvents.Add(evt);

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<EnvironmentalHealthStatus, bool> guard, EnvironmentalHealthStatus expected)
    {
        if (guard(Status)) return;

        throw new EnvironmentalHealthInvalidStatusTransitionException(
            EnvironmentalHealthId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
