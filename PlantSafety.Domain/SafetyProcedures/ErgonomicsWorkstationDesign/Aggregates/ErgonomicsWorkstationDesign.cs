// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.ErgonomicsWorkstationDesign
//  Layer:          Domain
//  File:           ErgonomicsWorkstationDesign.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single ergonomics/workstation-design procedure minimizing
//                  repetitive-motion injuries via adjustable workstation height/reach/layout,
//                  mechanical lifting aids, and job rotation. Models the real document/assessment
//                  lifecycle: drafting, approval, publication, assessment scheduling, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.ErgonomicsWorkstationDesign.Aggregates;

/// <summary>Aggregate root for a single ergonomics/workstation-design procedure.</summary>
public sealed partial class ErgonomicsWorkstationDesign
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this ErgonomicsWorkstationDesign aggregate.</summary>
    public ErgonomicsWorkstationDesignId ErgonomicsWorkstationDesignId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ErgonomicsWorkstationDesignStatus Status { get; private set; }

    // ---- Procedure identity ---------------------------------------------------------

    /// <summary>Human-readable title of this procedure document.</summary>
    public string ProcedureTitle { get; private set; } = string.Empty;

    /// <summary>Ergonomic workstation-design category.</summary>
    public WorkstationDesignCategory DesignCategory { get; private set; }

    /// <summary>Workstation/task location this procedure applies to.</summary>
    public string WorkstationLocation { get; private set; } = string.Empty;

    /// <summary>Identity of the approver.</summary>
    public UserId ApprovedByUserId { get; private set; }

    /// <summary>UTC timestamp the most recent assessment against this procedure was completed, if any.</summary>
    public UtcTimestamp? AssessedAtUtc { get; private set; }

    /// <summary>UTC timestamp the next assessment is due.</summary>
    public UtcTimestamp? NextAssessmentDueAtUtc { get; private set; }

    /// <summary>Number of risk factors identified during the most recent assessment.</summary>
    public int RiskFactorCount { get; private set; }

    /// <summary>Ergonomics score (0-100) from the most recent assessment.</summary>
    public decimal? ErgonomicsScore { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IErgonomicsWorkstationDesignDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IErgonomicsWorkstationDesignDomainEvent> DomainEvents
        => new ReadOnlyCollection<IErgonomicsWorkstationDesignDomainEvent>(_domainEvents);

    // For rehydration only.
    private ErgonomicsWorkstationDesign() { }

    // ---- Factory: Create (Draft) -----------------------------------------------------------

    /// <summary>Drafts a new ergonomics/workstation-design procedure and raises <see cref="ErgonomicsProcedureDraftedEvent"/>.</summary>
    public static ErgonomicsWorkstationDesign Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string procedureTitle = "",
        WorkstationDesignCategory designCategory = WorkstationDesignCategory.WorkstationHeight,
        string workstationLocation = "")
    {
        var agg = new ErgonomicsWorkstationDesign
        {
            ErgonomicsWorkstationDesignId = ErgonomicsWorkstationDesignId.New(),
            TenantId              = tenantId,
            SiteId                = siteId,
            Status                = ErgonomicsWorkstationDesignStatus.Draft,
            ProcedureTitle        = (procedureTitle ?? string.Empty).Trim(),
            DesignCategory        = designCategory,
            WorkstationLocation   = (workstationLocation ?? string.Empty).Trim(),
            CreatedAtUtc          = createdAtUtc,
            CreatedBy             = createdBy,
        };

        agg.AddDomainEvent(new ErgonomicsProcedureDraftedEvent(
            ErgonomicsWorkstationDesignEventId.New(), agg.ErgonomicsWorkstationDesignId, designCategory, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: SubmitForApproval -----------------------------------------------

    /// <summary>Submits the draft for approval.</summary>
    public void SubmitForApproval(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(ErgonomicsWorkstationDesignStatus.Draft);

        Status = ErgonomicsWorkstationDesignStatus.PendingApproval;

        AddDomainEvent(new ErgonomicsProcedureSubmittedForApprovalEvent(
            ErgonomicsWorkstationDesignEventId.New(), ErgonomicsWorkstationDesignId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Publish -----------------------------------------------------------

    /// <summary>Approves and publishes the procedure.</summary>
    public void Publish(UserId approvedByUserId, UtcTimestamp publishedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(ErgonomicsWorkstationDesignStatus.PendingApproval);

        ApprovedByUserId = approvedByUserId;
        Status = ErgonomicsWorkstationDesignStatus.Published;

        AddDomainEvent(new ErgonomicsProcedurePublishedEvent(
            ErgonomicsWorkstationDesignEventId.New(), ErgonomicsWorkstationDesignId, approvedByUserId, publishedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: ScheduleAssessment -----------------------------------------------

    /// <summary>Schedules a workstation assessment against this procedure.</summary>
    public void ScheduleAssessment(UtcTimestamp dueAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(ErgonomicsWorkstationDesignStatus.Published);

        NextAssessmentDueAtUtc = dueAtUtc;
        Status = ErgonomicsWorkstationDesignStatus.AssessmentScheduled;

        AddDomainEvent(new ErgonomicsWorkstationAssessmentScheduledEvent(
            ErgonomicsWorkstationDesignEventId.New(), ErgonomicsWorkstationDesignId, dueAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: RecordAssessment -----------------------------------------------

    /// <summary>Records the outcome of the scheduled assessment.</summary>
    public void RecordAssessment(int riskFactorCount, decimal ergonomicsScore, UtcTimestamp assessedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(ErgonomicsWorkstationDesignStatus.AssessmentScheduled);

        if (ergonomicsScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(ergonomicsScore), "Ergonomics score must be between 0 and 100.");

        RiskFactorCount = riskFactorCount;
        ErgonomicsScore = ergonomicsScore;
        AssessedAtUtc   = assessedAtUtc;
        Status          = ErgonomicsWorkstationDesignStatus.Published;

        AddDomainEvent(new ErgonomicsWorkstationAssessmentRecordedEvent(
            ErgonomicsWorkstationDesignEventId.New(), ErgonomicsWorkstationDesignId, riskFactorCount, ergonomicsScore, assessedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Retire -----------------------------------------------------------

    /// <summary>Retires the procedure document.</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == ErgonomicsWorkstationDesignStatus.Retired)
            throw new ErgonomicsWorkstationDesignInvalidStatusTransitionException(ErgonomicsWorkstationDesignId, Status.ToString(), ErgonomicsWorkstationDesignStatus.Retired.ToString(), correlationId, causationId);

        Status = ErgonomicsWorkstationDesignStatus.Retired;

        AddDomainEvent(new ErgonomicsProcedureRetiredEvent(
            ErgonomicsWorkstationDesignEventId.New(), ErgonomicsWorkstationDesignId, retiredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IErgonomicsWorkstationDesignDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ErgonomicsWorkstationDesignStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for ErgonomicsWorkstationDesign {ErgonomicsWorkstationDesignId}.");
    }
}
