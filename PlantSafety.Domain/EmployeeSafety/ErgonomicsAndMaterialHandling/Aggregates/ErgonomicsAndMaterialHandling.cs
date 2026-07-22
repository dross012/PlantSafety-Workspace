// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.ErgonomicsAndMaterialHandling
//  Layer:          Domain
//  File:           ErgonomicsAndMaterialHandling.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single ergonomic assessment covering a workstation or manual
//                  material-handling task. Reduces musculoskeletal injuries caused by repetitive
//                  motion, awkward posture, or heavy lifting. Models the real lifecycle: scheduling,
//                  assessment, control implementation, follow-up verification, and closure.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.ErgonomicsAndMaterialHandling.Aggregates;

/// <summary>Aggregate root for a single ergonomic assessment / material-handling risk review.</summary>
public sealed partial class ErgonomicsAndMaterialHandling
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this ErgonomicsAndMaterialHandling aggregate.</summary>
    public ErgonomicsAndMaterialHandlingId ErgonomicsAndMaterialHandlingId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ErgonomicsAndMaterialHandlingStatus Status { get; private set; }

    // ---- Assessment identity ---------------------------------------------------

    /// <summary>Human-readable title of this assessment (e.g. "Palletizing Station 2 - Lift Assessment").</summary>
    public string AssessmentTitle { get; private set; } = string.Empty;

    /// <summary>Primary ergonomic hazard category driving this assessment.</summary>
    public ErgonomicHazardCategory HazardCategory { get; private set; }

    /// <summary>Workstation/task location being assessed.</summary>
    public string WorkstationLocation { get; private set; } = string.Empty;

    /// <summary>Identity of the assessor.</summary>
    public UserId AssessedByUserId { get; private set; }

    /// <summary>UTC timestamp the assessment was completed, if recorded.</summary>
    public UtcTimestamp? AssessedAtUtc { get; private set; }

    /// <summary>Number of distinct risk factors identified (e.g. reach, force, frequency, duration).</summary>
    public int RiskFactorCount { get; private set; }

    /// <summary>Risk score (0-100, higher = higher risk) from the assessment.</summary>
    public decimal? RiskScore { get; private set; }

    /// <summary>Narrative description of controls implemented (mechanical aids, layout change, job rotation).</summary>
    public string? ControlsDescription { get; private set; }

    /// <summary>UTC timestamp the follow-up assessment is due, if scheduled.</summary>
    public UtcTimestamp? FollowUpDueAtUtc { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IErgonomicsAndMaterialHandlingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IErgonomicsAndMaterialHandlingDomainEvent> DomainEvents
        => new ReadOnlyCollection<IErgonomicsAndMaterialHandlingDomainEvent>(_domainEvents);

    // For rehydration only.
    private ErgonomicsAndMaterialHandling() { }

    // ---- Factory: Create -----------------------------------------------------------

    /// <summary>Schedules a new ergonomic assessment and raises <see cref="ErgonomicsAssessmentScheduledEvent"/>.</summary>
    public static ErgonomicsAndMaterialHandling Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string assessmentTitle = "",
        ErgonomicHazardCategory hazardCategory = ErgonomicHazardCategory.RepetitiveMotion,
        string workstationLocation = "",
        UserId assessedByUserId = default)
    {
        var agg = new ErgonomicsAndMaterialHandling
        {
            ErgonomicsAndMaterialHandlingId = ErgonomicsAndMaterialHandlingId.New(),
            TenantId              = tenantId,
            SiteId                = siteId,
            Status                = ErgonomicsAndMaterialHandlingStatus.AssessmentScheduled,
            AssessmentTitle       = (assessmentTitle ?? string.Empty).Trim(),
            HazardCategory        = hazardCategory,
            WorkstationLocation   = (workstationLocation ?? string.Empty).Trim(),
            AssessedByUserId      = assessedByUserId,
            CreatedAtUtc          = createdAtUtc,
            CreatedBy             = createdBy,
        };

        agg.AddDomainEvent(new ErgonomicsAssessmentScheduledEvent(
            ErgonomicsAndMaterialHandlingEventId.New(), agg.ErgonomicsAndMaterialHandlingId, hazardCategory,
            createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: RecordAssessment ---------------------------------------------

    /// <summary>Records the completed assessment's risk factors and score.</summary>
    public void RecordAssessment(int riskFactorCount, decimal riskScore, UtcTimestamp assessedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(ErgonomicsAndMaterialHandlingStatus.AssessmentScheduled, ErgonomicsAndMaterialHandlingStatus.FollowUpScheduled);

        if (riskScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(riskScore), "Risk score must be between 0 and 100.");

        RiskFactorCount = riskFactorCount;
        RiskScore       = riskScore;
        AssessedAtUtc   = assessedAtUtc;
        Status          = ErgonomicsAndMaterialHandlingStatus.Assessed;

        AddDomainEvent(new ErgonomicsAssessmentRecordedEvent(
            ErgonomicsAndMaterialHandlingEventId.New(), ErgonomicsAndMaterialHandlingId, riskFactorCount, riskScore,
            assessedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: ImplementControls ---------------------------------------------

    /// <summary>Records that mitigating controls (mechanical aids, layout change, job rotation) have been implemented.</summary>
    public void ImplementControls(string controlsDescription, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(ErgonomicsAndMaterialHandlingStatus.Assessed);

        ControlsDescription = controlsDescription;
        Status = ErgonomicsAndMaterialHandlingStatus.ControlsImplemented;

        AddDomainEvent(new ErgonomicsControlsImplementedEvent(
            ErgonomicsAndMaterialHandlingEventId.New(), ErgonomicsAndMaterialHandlingId, controlsDescription,
            occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: ScheduleFollowUp -----------------------------------------------

    /// <summary>Schedules a follow-up assessment to verify control effectiveness.</summary>
    public void ScheduleFollowUp(UtcTimestamp dueAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(ErgonomicsAndMaterialHandlingStatus.ControlsImplemented);

        FollowUpDueAtUtc = dueAtUtc;
        Status = ErgonomicsAndMaterialHandlingStatus.FollowUpScheduled;

        AddDomainEvent(new ErgonomicsFollowUpScheduledEvent(
            ErgonomicsAndMaterialHandlingEventId.New(), ErgonomicsAndMaterialHandlingId, dueAtUtc,
            actor, correlationId, causationId));
    }

    // ---- Behavior: Close -----------------------------------------------------------

    /// <summary>Closes the assessment once risk has been mitigated to an acceptable level.</summary>
    public void Close(UtcTimestamp closedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(ErgonomicsAndMaterialHandlingStatus.ControlsImplemented, ErgonomicsAndMaterialHandlingStatus.FollowUpScheduled);

        Status = ErgonomicsAndMaterialHandlingStatus.Closed;

        AddDomainEvent(new ErgonomicsAssessmentClosedEvent(
            ErgonomicsAndMaterialHandlingEventId.New(), ErgonomicsAndMaterialHandlingId, closedAtUtc,
            actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IErgonomicsAndMaterialHandlingDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ErgonomicsAndMaterialHandlingStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for ErgonomicsAndMaterialHandling {ErgonomicsAndMaterialHandlingId}.");
    }
}
