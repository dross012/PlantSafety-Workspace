// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.EmergencyPreparedness
//  Layer:          Domain
//  File:           EmergencyPreparedness.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single emergency response plan covering one scenario type
//                  (fire, chemical spill, severe weather, medical emergency). Ensures employees can
//                  respond quickly and correctly via evacuation routes, drills, and alarm testing.
//                  Models the real lifecycle: documentation, drill scheduling, drill execution, and
//                  readiness verification.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.EmergencyPreparedness.Aggregates;

/// <summary>Aggregate root for a single emergency response plan and its drill/readiness cycle.</summary>
public sealed partial class EmergencyPreparedness
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this EmergencyPreparedness aggregate.</summary>
    public EmergencyPreparednessId EmergencyPreparednessId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public EmergencyPreparednessStatus Status { get; private set; }

    // ---- Plan identity ---------------------------------------------------------

    /// <summary>Human-readable title of the plan.</summary>
    public string PlanTitle { get; private set; } = string.Empty;

    /// <summary>Emergency scenario type this plan addresses.</summary>
    public EmergencyType EmergencyType { get; private set; }

    /// <summary>Documented evacuation route or response procedure reference.</summary>
    public string EvacuationRoute { get; private set; } = string.Empty;

    /// <summary>Identity of the employee/coordinator responsible for this plan.</summary>
    public UserId CoordinatedByUserId { get; private set; }

    /// <summary>UTC timestamp the most recent drill was conducted, if any.</summary>
    public UtcTimestamp? DrillConductedAtUtc { get; private set; }

    /// <summary>UTC timestamp the next drill is due.</summary>
    public UtcTimestamp? NextDrillDueAtUtc { get; private set; }

    /// <summary>Number of employees who participated in the most recent drill.</summary>
    public int ParticipantCount { get; private set; }

    /// <summary>Readiness score (0-100) derived from the most recent drill.</summary>
    public decimal? ReadinessScore { get; private set; }

    /// <summary>Minimum acceptable readiness score.</summary>
    public decimal ReadinessThreshold { get; private set; } = 80m;

    /// <summary>Narrative notes describing a readiness gap, if identified.</summary>
    public string? ReadinessGapNotes { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IEmergencyPreparednessDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IEmergencyPreparednessDomainEvent> DomainEvents
        => new ReadOnlyCollection<IEmergencyPreparednessDomainEvent>(_domainEvents);

    // For rehydration only.
    private EmergencyPreparedness() { }

    // ---- Factory: Create -----------------------------------------------------------

    /// <summary>Documents a new emergency response plan and raises <see cref="EmergencyPreparednessDocumentedEvent"/>.</summary>
    public static EmergencyPreparedness Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string planTitle = "",
        EmergencyType emergencyType = EmergencyType.Fire,
        string evacuationRoute = "",
        UserId coordinatedByUserId = default,
        decimal readinessThreshold = 80m)
    {
        var agg = new EmergencyPreparedness
        {
            EmergencyPreparednessId = EmergencyPreparednessId.New(),
            TenantId            = tenantId,
            SiteId              = siteId,
            Status              = EmergencyPreparednessStatus.Documented,
            PlanTitle           = (planTitle ?? string.Empty).Trim(),
            EmergencyType       = emergencyType,
            EvacuationRoute     = (evacuationRoute ?? string.Empty).Trim(),
            CoordinatedByUserId = coordinatedByUserId,
            ReadinessThreshold  = readinessThreshold,
            CreatedAtUtc        = createdAtUtc,
            CreatedBy           = createdBy,
        };

        agg.AddDomainEvent(new EmergencyPreparednessDocumentedEvent(
            EmergencyPreparednessEventId.New(), agg.EmergencyPreparednessId, emergencyType,
            createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: ScheduleDrill ---------------------------------------------

    /// <summary>Schedules the next emergency drill.</summary>
    public void ScheduleDrill(UtcTimestamp dueAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(EmergencyPreparednessStatus.Documented, EmergencyPreparednessStatus.ReadinessVerified, EmergencyPreparednessStatus.ReadinessGapIdentified);

        NextDrillDueAtUtc = dueAtUtc;
        Status = EmergencyPreparednessStatus.DrillScheduled;

        AddDomainEvent(new EmergencyPreparednessDrillScheduledEvent(
            EmergencyPreparednessEventId.New(), EmergencyPreparednessId, dueAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: ConductDrill ---------------------------------------------

    /// <summary>Records that a scheduled drill was conducted.</summary>
    public void ConductDrill(int participantCount, UtcTimestamp conductedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(EmergencyPreparednessStatus.DrillScheduled);

        if (participantCount < 0)
            throw new ArgumentOutOfRangeException(nameof(participantCount), "Participant count cannot be negative.");

        ParticipantCount    = participantCount;
        DrillConductedAtUtc = conductedAtUtc;
        Status = EmergencyPreparednessStatus.DrillConducted;

        AddDomainEvent(new EmergencyPreparednessDrillConductedEvent(
            EmergencyPreparednessEventId.New(), EmergencyPreparednessId, participantCount, conductedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: RecordReadiness ---------------------------------------------

    /// <summary>Records the readiness score derived from the conducted drill.</summary>
    public void RecordReadiness(decimal readinessScore, string? gapNotes, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(EmergencyPreparednessStatus.DrillConducted);

        if (readinessScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(readinessScore), "Readiness score must be between 0 and 100.");

        ReadinessScore = readinessScore;

        if (readinessScore >= ReadinessThreshold)
        {
            Status = EmergencyPreparednessStatus.ReadinessVerified;
            AddDomainEvent(new EmergencyPreparednessReadinessVerifiedEvent(
                EmergencyPreparednessEventId.New(), EmergencyPreparednessId, readinessScore, occurredAtUtc, actor, correlationId, causationId));
        }
        else
        {
            ReadinessGapNotes = gapNotes;
            Status = EmergencyPreparednessStatus.ReadinessGapIdentified;
            AddDomainEvent(new EmergencyPreparednessReadinessGapIdentifiedEvent(
                EmergencyPreparednessEventId.New(), EmergencyPreparednessId, readinessScore, gapNotes ?? string.Empty, occurredAtUtc, actor, correlationId, causationId));
        }
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IEmergencyPreparednessDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params EmergencyPreparednessStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for EmergencyPreparedness {EmergencyPreparednessId}.");
    }
}
