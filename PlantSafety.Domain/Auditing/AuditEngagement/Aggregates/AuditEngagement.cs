// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement
//  Layer:          Domain
//  File:           AuditEngagement.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root representing a single feature-level manufacturing audit engagement (Operations, Systems, Procedures, Quality Control, Regulatory Compliance, Efficiency, Inspecting, or Facility Assessment). NOT an application/software/code audit.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditEngagement.Aggregates;

/// <summary>Aggregate root for the AuditEngagement bounded context.</summary>
public sealed partial class AuditEngagement
{
    // ---- Identity ------------------------------------------------------

    /// <summary>Strong-typed identity of this AuditEngagement aggregate.</summary>
    public AuditEngagementId AuditEngagementId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public AuditEngagementStatus Status { get; private set; }

    // ---- Business properties --------------------------------------------
    /// <summary>Human-readable audit engagement title.</summary>
    public string EngagementTitle { get; private set; } = string.Empty;

    /// <summary>Audit discipline code (see AuditDisciplineType smart enum): Operations, Systems, Procedures, QualityControl, RegulatoryCompliance, Efficiency, Inspecting, FacilityAssessment.</summary>
    public string DisciplineType { get; private set; } = string.Empty;

    /// <summary>Narrative scope of the audit (areas, processes, sites covered).</summary>
    public string Scope { get; private set; } = string.Empty;

    /// <summary>Identity of the lead auditor assigned to this engagement.</summary>
    public UserId LeadAuditorId { get; private set; }

    /// <summary>Scheduled execution date/time for the audit, if planned.</summary>
    public UtcTimestamp? ScheduledAtUtc { get; private set; }

    /// <summary>UTC timestamp the audit was completed, if finished.</summary>
    public UtcTimestamp? CompletedAtUtc { get; private set; }

    /// <summary>Count of findings raised during this engagement.</summary>
    public int FindingsCount { get; private set; }

    /// <summary>Computed compliance score (0-100) for this engagement, when available.</summary>
    public decimal? ComplianceScore { get; private set; }

    /// <summary>Overall compliance rating classification: NotRated, Satisfactory, NeedsImprovement, Unsatisfactory.</summary>
    public string ComplianceRating { get; private set; } = "NotRated";

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------

    private readonly List<IAuditEngagementDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAuditEngagementDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAuditEngagementDomainEvent>(_domainEvents);

    // For rehydration only.
    private AuditEngagement() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new AuditEngagement aggregate and raises <see cref="AuditEngagementCreatedEvent"/>.</summary>
    /// <param name="createdBy">CRUD authorship identity.</param>
    /// <param name="createdAtUtc">Creation timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="siteId">Optional owning site.</param>
    public static AuditEngagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string engagementTitle = "",
        string disciplineType = "",
        string scope = "",
        UserId leadAuditorId = default,
        UtcTimestamp? scheduledAtUtc = null,
        int findingsCount = 0,
        decimal? complianceScore = null,
        string complianceRating = "NotRated")
    {
        var agg = new AuditEngagement
        {
            AuditEngagementId      = AuditEngagementId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = AuditEngagementStatus.Created,
            EngagementTitle  = (engagementTitle ?? string.Empty).Trim(),
            DisciplineType   = (disciplineType ?? string.Empty).Trim(),
            Scope            = (scope ?? string.Empty).Trim(),
            LeadAuditorId    = leadAuditorId,
            ScheduledAtUtc   = scheduledAtUtc,
            FindingsCount    = findingsCount,
            ComplianceScore  = complianceScore,
            ComplianceRating = (complianceRating ?? "NotRated").Trim(),
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new AuditEngagementCreatedEvent(
            EventId:       new AuditEngagementEventId(Uuid7.New()),
            AuditEngagementId:      agg.AuditEngagementId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle behavior --------------------------------------------------

    /// <summary>
    /// Plans and schedules this engagement (Audit Workflow Step 1: Planning). Assigns the lead
    /// auditor and sets the execution date. (Created → Scheduled).
    /// </summary>
    /// <exception cref="AuditEngagementInvalidStatusTransitionException">Thrown when the engagement has already been scheduled or started.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="scheduledAtUtc"/> is empty.</exception>
    public void Schedule(
        UserId leadAuditorId,
        UtcTimestamp scheduledAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        UtcTimestamp occurredAtUtc)
    {
        if (Status != AuditEngagementStatus.Created)
            throw new AuditEngagementInvalidStatusTransitionException(
                AuditEngagementId, Status.ToString(), AuditEngagementStatus.Scheduled.ToString(), correlationId, causationId);

        if (scheduledAtUtc.IsEmpty)
            throw new ArgumentException("ScheduledAtUtc must be provided.", nameof(scheduledAtUtc));

        LeadAuditorId = leadAuditorId;
        ScheduledAtUtc = scheduledAtUtc;
        Status = AuditEngagementStatus.Scheduled;

        AddDomainEvent(new AuditEngagementScheduledEvent(
            EventId: new AuditEngagementEventId(Uuid7.New()),
            AuditEngagementId: AuditEngagementId,
            LeadAuditorId: leadAuditorId,
            ScheduledForUtc: scheduledAtUtc,
            PlannedAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Begins field execution of this engagement (Audit Workflow Step 2: Execution). (Scheduled → Active).</summary>
    /// <exception cref="AuditEngagementInvalidStatusTransitionException">Thrown when the engagement has not been scheduled.</exception>
    public void Start(
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        UtcTimestamp occurredAtUtc)
    {
        EnsureStatusIsOneOf(AuditEngagementStatus.Scheduled, AuditEngagementStatus.Created);

        Status = AuditEngagementStatus.Active;

        AddDomainEvent(new AuditEngagementStartedEvent(
            EventId: new AuditEngagementEventId(Uuid7.New()),
            AuditEngagementId: AuditEngagementId,
            StartedAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Records that a new <see cref="Auditing.AuditFinding.Aggregates.AuditFinding"/> has been
    /// raised against this engagement (Audit Workflow Step 3: Findings), incrementing the
    /// running findings count. Must be called while the engagement is Active.
    /// </summary>
    /// <exception cref="AuditEngagementInvalidStatusTransitionException">Thrown when the engagement is not in progress.</exception>
    public void RecordFinding(
        Guid auditFindingId,
        string severity,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        UtcTimestamp occurredAtUtc)
    {
        if (Status != AuditEngagementStatus.Active)
            throw new AuditEngagementInvalidStatusTransitionException(
                AuditEngagementId, Status.ToString(), "RecordFinding", correlationId, causationId);

        FindingsCount++;

        AddDomainEvent(new AuditEngagementFindingRecordedEvent(
            EventId: new AuditEngagementEventId(Uuid7.New()),
            AuditEngagementId: AuditEngagementId,
            AuditFindingId: auditFindingId,
            Severity: (severity ?? "Medium").Trim(),
            RunningFindingsCount: FindingsCount,
            RecordedAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Closes this engagement (Audit Workflow Step 5: Reporting), recording the final compliance
    /// score/rating and completion timestamp. (Active → Closed).
    /// </summary>
    /// <exception cref="AuditEngagementInvalidStatusTransitionException">Thrown when the engagement is not in progress.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="complianceScore"/> is outside 0-100.</exception>
    public void Close(
        decimal? complianceScore,
        string complianceRating,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        UtcTimestamp occurredAtUtc)
    {
        if (Status != AuditEngagementStatus.Active)
            throw new AuditEngagementInvalidStatusTransitionException(
                AuditEngagementId, Status.ToString(), AuditEngagementStatus.Closed.ToString(), correlationId, causationId);

        if (complianceScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(complianceScore), complianceScore, "ComplianceScore must be between 0 and 100.");

        ComplianceScore = complianceScore;
        ComplianceRating = (complianceRating ?? "NotRated").Trim();
        CompletedAtUtc = occurredAtUtc;
        Status = AuditEngagementStatus.Closed;

        AddDomainEvent(new AuditEngagementClosedEvent(
            EventId: new AuditEngagementEventId(Uuid7.New()),
            AuditEngagementId: AuditEngagementId,
            ComplianceScore: ComplianceScore,
            ComplianceRating: ComplianceRating,
            FindingsCount: FindingsCount,
            ClosedAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IAuditEngagementDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params AuditEngagementStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            "Invalid lifecycle transition from '" + Status + "' for AuditEngagement " + AuditEngagementId + ".");
    }
}