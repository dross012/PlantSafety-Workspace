// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.EmergencyResponsePlanning
//  Layer:          Domain
//  File:           EmergencyResponsePlanning.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single emergency response planning procedure document covering
//                  one scenario type (fire, chemical spill, severe weather, medical emergency). Models
//                  the real document/drill lifecycle: drafting, approval, publication, drill
//                  scheduling/execution, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.EmergencyResponsePlanning.Aggregates;

/// <summary>Aggregate root for a single emergency response planning procedure document.</summary>
public sealed partial class EmergencyResponsePlanning
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this EmergencyResponsePlanning aggregate.</summary>
    public EmergencyResponsePlanningId EmergencyResponsePlanningId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public EmergencyResponsePlanningStatus Status { get; private set; }

    // ---- Plan identity ---------------------------------------------------------

    /// <summary>Human-readable title of this plan.</summary>
    public string PlanTitle { get; private set; } = string.Empty;

    /// <summary>Emergency scenario type this plan addresses.</summary>
    public ProcedureEmergencyType EmergencyType { get; private set; }

    /// <summary>Documented evacuation route.</summary>
    public string EvacuationRoute { get; private set; } = string.Empty;

    /// <summary>Identity of the approver.</summary>
    public UserId ApprovedByUserId { get; private set; }

    /// <summary>UTC timestamp the most recent drill was conducted, if any.</summary>
    public UtcTimestamp? DrillConductedAtUtc { get; private set; }

    /// <summary>UTC timestamp the next drill is due.</summary>
    public UtcTimestamp? NextDrillDueAtUtc { get; private set; }

    /// <summary>Number of employees who participated in the most recent drill.</summary>
    public int ParticipantCount { get; private set; }

    /// <summary>Readiness score (0-100) derived from the most recent drill.</summary>
    public decimal? ReadinessScore { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IEmergencyResponsePlanningDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IEmergencyResponsePlanningDomainEvent> DomainEvents
        => new ReadOnlyCollection<IEmergencyResponsePlanningDomainEvent>(_domainEvents);

    // For rehydration only.
    private EmergencyResponsePlanning() { }

    // ---- Factory: Create (Draft) -----------------------------------------------------------

    /// <summary>Drafts a new emergency response plan and raises <see cref="EmergencyPlanDraftedEvent"/>.</summary>
    public static EmergencyResponsePlanning Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string planTitle = "",
        ProcedureEmergencyType emergencyType = ProcedureEmergencyType.Fire,
        string evacuationRoute = "")
    {
        var agg = new EmergencyResponsePlanning
        {
            EmergencyResponsePlanningId = EmergencyResponsePlanningId.New(),
            TenantId         = tenantId,
            SiteId           = siteId,
            Status           = EmergencyResponsePlanningStatus.Draft,
            PlanTitle        = (planTitle ?? string.Empty).Trim(),
            EmergencyType    = emergencyType,
            EvacuationRoute  = (evacuationRoute ?? string.Empty).Trim(),
            CreatedAtUtc     = createdAtUtc,
            CreatedBy        = createdBy,
        };

        agg.AddDomainEvent(new EmergencyPlanDraftedEvent(
            EmergencyResponsePlanningEventId.New(), agg.EmergencyResponsePlanningId, emergencyType, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: SubmitForApproval -----------------------------------------------

    /// <summary>Submits the draft for approval.</summary>
    public void SubmitForApproval(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(EmergencyResponsePlanningStatus.Draft);

        Status = EmergencyResponsePlanningStatus.PendingApproval;

        AddDomainEvent(new EmergencyPlanSubmittedForApprovalEvent(
            EmergencyResponsePlanningEventId.New(), EmergencyResponsePlanningId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Publish -----------------------------------------------------------

    /// <summary>Approves and publishes the plan.</summary>
    public void Publish(UserId approvedByUserId, UtcTimestamp publishedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(EmergencyResponsePlanningStatus.PendingApproval);

        ApprovedByUserId = approvedByUserId;
        Status = EmergencyResponsePlanningStatus.Published;

        AddDomainEvent(new EmergencyPlanPublishedEvent(
            EmergencyResponsePlanningEventId.New(), EmergencyResponsePlanningId, approvedByUserId, publishedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: ScheduleDrill -----------------------------------------------

    /// <summary>Schedules the next drill.</summary>
    public void ScheduleDrill(UtcTimestamp dueAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(EmergencyResponsePlanningStatus.Published);

        NextDrillDueAtUtc = dueAtUtc;
        Status = EmergencyResponsePlanningStatus.DrillScheduled;

        AddDomainEvent(new EmergencyPlanDrillScheduledEvent(
            EmergencyResponsePlanningEventId.New(), EmergencyResponsePlanningId, dueAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: RecordDrill -----------------------------------------------

    /// <summary>Records the outcome of the conducted drill.</summary>
    public void RecordDrill(int participantCount, decimal readinessScore, UtcTimestamp conductedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(EmergencyResponsePlanningStatus.DrillScheduled);

        if (readinessScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(readinessScore), "Readiness score must be between 0 and 100.");

        ParticipantCount    = participantCount;
        ReadinessScore      = readinessScore;
        DrillConductedAtUtc = conductedAtUtc;
        Status              = EmergencyResponsePlanningStatus.Published;

        AddDomainEvent(new EmergencyPlanDrillRecordedEvent(
            EmergencyResponsePlanningEventId.New(), EmergencyResponsePlanningId, participantCount, readinessScore, conductedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Retire -----------------------------------------------------------

    /// <summary>Retires the plan document.</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == EmergencyResponsePlanningStatus.Retired)
            throw new EmergencyResponsePlanningInvalidStatusTransitionException(EmergencyResponsePlanningId, Status.ToString(), EmergencyResponsePlanningStatus.Retired.ToString(), correlationId, causationId);

        Status = EmergencyResponsePlanningStatus.Retired;

        AddDomainEvent(new EmergencyPlanRetiredEvent(
            EmergencyResponsePlanningEventId.New(), EmergencyResponsePlanningId, retiredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IEmergencyResponsePlanningDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params EmergencyResponsePlanningStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for EmergencyResponsePlanning {EmergencyResponsePlanningId}.");
    }
}
