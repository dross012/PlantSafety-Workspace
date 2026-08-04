// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin.EvacuationModeling.EgressSimulations
//  Layer:          Domain
//  File:           EgressSimulationRun.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a single egress-simulation run (Egress Simulation and
//                  Evacuation Drill Correlation subdomains). Models the real lifecycle: a run is
//                  started against a validated Digital Twin model version and a trigger reason
//                  (scheduled re-simulation on twin-model change, quarterly full-site simulation,
//                  drill-triggered replay, or manual), completes (or fails), and may optionally be
//                  linked back to a SafetyEvents.DrillsAndExercises drill for correlation.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.EvacuationModeling.EgressSimulations.Aggregates;

/// <summary>Aggregate root for a single egress-simulation run.</summary>
public sealed partial class EgressSimulationRun
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this EgressSimulationRun aggregate.</summary>
    public EgressSimulationRunId EgressSimulationRunId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public EgressSimulationStatus Status { get; private set; }

    // ---- Run identity --------------------------------------------------------

    /// <summary>Identity of the evacuation zone this simulation run models.</summary>
    public EvacuationZoneId ZoneId { get; private set; }

    /// <summary>Version of the Digital Twin model this run's geometry was sourced from.</summary>
    public string TwinModelVersion { get; private set; } = string.Empty;

    /// <summary>Reason this run was triggered.</summary>
    public EgressSimulationTrigger TriggerReason { get; private set; }

    /// <summary>Simulated total evacuation time in seconds, populated once <see cref="Complete"/> is called.</summary>
    public int? SimulatedEvacuationTimeSeconds { get; private set; }

    /// <summary>Predicted peak occupancy considered by the simulation, if supplied.</summary>
    public int? PredictedPeakOccupancy { get; private set; }

    /// <summary>Identity of the drill (SafetyEvents.DrillsAndExercises) this run is correlated with, if any.</summary>
    public Guid? LinkedDrillId { get; private set; }

    /// <summary>UTC timestamp the run started.</summary>
    public UtcTimestamp StartedAtUtc { get; private set; }

    /// <summary>UTC timestamp the run completed or failed, if it has.</summary>
    public UtcTimestamp? CompletedAtUtc { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IEgressSimulationRunDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IEgressSimulationRunDomainEvent> DomainEvents
        => new ReadOnlyCollection<IEgressSimulationRunDomainEvent>(_domainEvents);

    // For rehydration only.
    private EgressSimulationRun() { }

    // ---- Factory: Create (Start) -----------------------------------------------------------

    /// <summary>Starts a new egress-simulation run against a validated Digital Twin model version and raises <see cref="EgressSimulationStartedEvent"/>.</summary>
    public static EgressSimulationRun Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        EvacuationZoneId zoneId = default,
        string twinModelVersion = "",
        EgressSimulationTrigger triggerReason = EgressSimulationTrigger.Manual)
    {
        if (string.IsNullOrWhiteSpace(twinModelVersion))
            throw new ArgumentException("A validated Digital Twin model version is required to run a simulation.", nameof(twinModelVersion));

        var agg = new EgressSimulationRun
        {
            EgressSimulationRunId = EgressSimulationRunId.New(),
            TenantId              = tenantId,
            SiteId                = siteId,
            Status                = EgressSimulationStatus.Running,
            ZoneId                = zoneId,
            TwinModelVersion      = twinModelVersion.Trim(),
            TriggerReason         = triggerReason,
            StartedAtUtc          = createdAtUtc,
            CreatedAtUtc          = createdAtUtc,
            CreatedBy             = createdBy,
        };

        agg.AddDomainEvent(new EgressSimulationStartedEvent(
            EgressSimulationEventId.New(), agg.EgressSimulationRunId, agg.ZoneId, triggerReason, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: Complete -----------------------------------------------

    /// <summary>Completes this simulation run with the computed results and raises <see cref="EgressSimulationCompletedEvent"/>.</summary>
    public void Complete(
        int simulatedEvacuationTimeSeconds, int? predictedPeakOccupancy,
        UtcTimestamp completedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(EgressSimulationStatus.Running);

        if (simulatedEvacuationTimeSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(simulatedEvacuationTimeSeconds), "Simulated evacuation time must not be negative.");

        Status                         = EgressSimulationStatus.Completed;
        SimulatedEvacuationTimeSeconds = simulatedEvacuationTimeSeconds;
        PredictedPeakOccupancy         = predictedPeakOccupancy;
        CompletedAtUtc                 = completedAtUtc;

        AddDomainEvent(new EgressSimulationCompletedEvent(
            EgressSimulationEventId.New(), EgressSimulationRunId, ZoneId, simulatedEvacuationTimeSeconds, completedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Fail -----------------------------------------------

    /// <summary>Marks this simulation run as failed.</summary>
    public void Fail(UtcTimestamp failedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(EgressSimulationStatus.Running);

        Status         = EgressSimulationStatus.Failed;
        CompletedAtUtc = failedAtUtc;

        AddDomainEvent(new EgressSimulationFailedEvent(
            EgressSimulationEventId.New(), EgressSimulationRunId, failedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: LinkToDrill -----------------------------------------------

    /// <summary>Correlates this simulation run to a SafetyEvents.DrillsAndExercises drill and raises <see cref="EgressSimulationLinkedToDrillEvent"/>.</summary>
    public void LinkToDrill(Guid drillId, UtcTimestamp linkedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (drillId == Guid.Empty)
            throw new ArgumentException("DrillId must not be empty.", nameof(drillId));

        LinkedDrillId = drillId;

        AddDomainEvent(new EgressSimulationLinkedToDrillEvent(
            EgressSimulationEventId.New(), EgressSimulationRunId, drillId, linkedAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IEgressSimulationRunDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params EgressSimulationStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for EgressSimulationRun {EgressSimulationRunId}.");
    }
}
