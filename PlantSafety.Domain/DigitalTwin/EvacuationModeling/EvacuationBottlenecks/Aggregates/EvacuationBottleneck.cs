// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EvacuationBottlenecks
//  Layer:          Domain
//  File:           EvacuationBottleneck.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single detected evacuation bottleneck (Bottleneck
//                  Analysis subdomain). Raised from EgressSimulationBottleneckAnalysisService when
//                  a completed egress-simulation run's simulated time materially exceeds a route's
//                  estimated egress time. Tracks mitigation status and recurrence across
//                  subsequent simulation runs -- feeding the bottleneck-recurrence-rate KPI.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EvacuationBottlenecks.Aggregates;

/// <summary>Aggregate root for a single detected evacuation bottleneck.</summary>
public sealed partial class EvacuationBottleneck
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this EvacuationBottleneck aggregate.</summary>
    public EvacuationBottleneckId EvacuationBottleneckId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current status.</summary>
    public EvacuationBottleneckStatus Status { get; private set; }

    // ---- Bottleneck identity --------------------------------------------------------

    /// <summary>Identity of the evacuation route the bottleneck was detected on.</summary>
    public EvacuationRouteId RouteId { get; private set; }

    /// <summary>Identity of the egress-simulation run that surfaced this bottleneck.</summary>
    public EgressSimulationRunId SimulationRunId { get; private set; }

    /// <summary>Free-text description of the bottleneck location within the route.</summary>
    public string BottleneckLocation { get; private set; } = string.Empty;

    /// <summary>Delay, in seconds, above the route's estimated egress time observed by the simulation.</summary>
    public int DelaySeconds { get; private set; }

    /// <summary>Number of times this bottleneck has been detected again on subsequent simulation runs.</summary>
    public int RecurrenceCount { get; private set; }

    /// <summary>UTC timestamp the bottleneck was first detected.</summary>
    public UtcTimestamp DetectedAtUtc { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IEvacuationBottleneckDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IEvacuationBottleneckDomainEvent> DomainEvents
        => new ReadOnlyCollection<IEvacuationBottleneckDomainEvent>(_domainEvents);

    // For rehydration only.
    private EvacuationBottleneck() { }

    // ---- Factory: Create (Detect) -----------------------------------------------------------

    /// <summary>Records a newly detected evacuation bottleneck and raises <see cref="EvacuationBottleneckDetectedEvent"/>.</summary>
    public static EvacuationBottleneck Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        EvacuationRouteId routeId = default,
        EgressSimulationRunId simulationRunId = default,
        string bottleneckLocation = "",
        int delaySeconds = 0)
    {
        var agg = new EvacuationBottleneck
        {
            EvacuationBottleneckId = EvacuationBottleneckId.New(),
            TenantId               = tenantId,
            SiteId                 = siteId,
            Status                 = EvacuationBottleneckStatus.Detected,
            RouteId                = routeId,
            SimulationRunId        = simulationRunId,
            BottleneckLocation     = (bottleneckLocation ?? string.Empty).Trim(),
            DelaySeconds           = delaySeconds,
            RecurrenceCount        = 0,
            DetectedAtUtc          = createdAtUtc,
            CreatedAtUtc           = createdAtUtc,
            CreatedBy              = createdBy,
        };

        agg.AddDomainEvent(new EvacuationBottleneckDetectedEvent(
            EvacuationBottleneckEventId.New(), agg.EvacuationBottleneckId, routeId, simulationRunId, delaySeconds, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: RecordRecurrence -----------------------------------------------

    /// <summary>Records that this bottleneck recurred on a subsequent simulation run and raises <see cref="EvacuationBottleneckRecurredEvent"/>.</summary>
    public void RecordRecurrence(
        EgressSimulationRunId simulationRunId, int delaySeconds, UtcTimestamp recurredAtUtc,
        PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        RecurrenceCount++;
        SimulationRunId = simulationRunId;
        DelaySeconds     = delaySeconds;
        Status           = EvacuationBottleneckStatus.Recurred;

        AddDomainEvent(new EvacuationBottleneckRecurredEvent(
            EvacuationBottleneckEventId.New(), EvacuationBottleneckId, simulationRunId, RecurrenceCount, recurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: MarkMitigated -----------------------------------------------

    /// <summary>Marks this bottleneck as mitigated (e.g. after a corrective action to widen or reroute the affected segment).</summary>
    public void MarkMitigated(UtcTimestamp mitigatedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        Status = EvacuationBottleneckStatus.Mitigated;

        AddDomainEvent(new EvacuationBottleneckMitigatedEvent(
            EvacuationBottleneckEventId.New(), EvacuationBottleneckId, mitigatedAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IEvacuationBottleneckDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
