// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Domain
//  File:           Auditing.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 — extended from identity-only scaffold with the full
//                  audit-program record shape (title, type, scope, auditor, schedule,
//                  findings, score) grounded in project_plantsafety_domains.md Module 2
//                  (Auditing: "Audit programs, execution, findings, scoring, and
//                  corrective actions"). Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Auditing bounded context. Tracks a single
//                  audit program/execution record: its type, scope, assigned auditor,
//                  schedule, findings volume, and computed compliance score.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Auditing.Aggregates;

/// <summary>Aggregate root for the Auditing bounded context.</summary>
public sealed partial class Auditing
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Auditing aggregate.</summary>
    public AuditingId AuditingId { get; private set; }

    /// <summary>Tenant that owns this audit record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this audit is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public AuditingStatus Status { get; private set; }

    // ---- Audit-specific business properties --------------------------------

    /// <summary>Human-readable audit program/execution title.</summary>
    public string AuditTitle { get; private set; } = string.Empty;

    /// <summary>Audit classification: Internal, External, Regulatory, Supplier.</summary>
    public string AuditType { get; private set; } = "Internal";

    /// <summary>Narrative scope of the audit (areas, processes, sites covered).</summary>
    public string Scope { get; private set; } = string.Empty;

    /// <summary>Identity of the lead auditor assigned to this audit.</summary>
    public UserId AuditorId { get; private set; }

    /// <summary>Scheduled execution date/time for the audit, if planned.</summary>
    public UtcTimestamp? ScheduledAtUtc { get; private set; }

    /// <summary>UTC timestamp the audit was completed, if finished.</summary>
    public UtcTimestamp? CompletedAtUtc { get; private set; }

    /// <summary>Count of findings raised during this audit.</summary>
    public int FindingsCount { get; private set; }

    /// <summary>Computed compliance score (0-100) for this audit, when available.</summary>
    public decimal? Score { get; private set; }

    /// <summary>Overall compliance rating classification: NotRated, Satisfactory, NeedsImprovement, Unsatisfactory.</summary>
    public string ComplianceRating { get; private set; } = "NotRated";

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IAuditingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAuditingDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAuditingDomainEvent>(_domainEvents);

    // For rehydration only.
    private Auditing() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Auditing aggregate and raises <see cref="AuditingCreatedEvent"/>.</summary>
    /// <param name="createdBy">CRUD authorship identity.</param>
    /// <param name="createdAtUtc">Creation timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="siteId">Optional owning site.</param>
    /// <param name="auditTitle">Audit program/execution title.</param>
    /// <param name="auditType">Audit classification (Internal/External/Regulatory/Supplier).</param>
    /// <param name="scope">Narrative scope of the audit.</param>
    /// <param name="auditorId">Lead auditor identity.</param>
    /// <param name="scheduledAtUtc">Optional scheduled execution timestamp.</param>
    /// <param name="findingsCount">Initial findings count. Defaults to zero.</param>
    /// <param name="score">Optional initial compliance score (0-100).</param>
    /// <param name="complianceRating">Overall compliance rating classification.</param>
    public static Auditing Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string auditTitle = "",
        string auditType = "Internal",
        string scope = "",
        UserId auditorId = default,
        UtcTimestamp? scheduledAtUtc = null,
        int findingsCount = 0,
        decimal? score = null,
        string complianceRating = "NotRated")
    {
        var agg = new Auditing
        {
            AuditingId       = AuditingId.New(),
            TenantId         = tenantId,
            SiteId           = siteId,
            Status           = AuditingStatus.Created,
            AuditTitle       = (auditTitle ?? string.Empty).Trim(),
            AuditType        = (auditType ?? "Internal").Trim(),
            Scope            = (scope ?? string.Empty).Trim(),
            AuditorId        = auditorId,
            ScheduledAtUtc   = scheduledAtUtc,
            FindingsCount    = findingsCount,
            Score            = score,
            ComplianceRating = (complianceRating ?? "NotRated").Trim(),
            CreatedAtUtc     = createdAtUtc,
            CreatedBy        = createdBy,
        };

        agg.AddDomainEvent(new AuditingCreatedEvent(
            EventId:       new AuditingEventId(Uuid7.New()),
            AuditingId:      agg.AuditingId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: Start / RecordFindings / Close / Archive ---------------
    // Phase 4 full build-out (2026-08-02): this aggregate previously had only the Create
    // factory -- AuditingStatus/AuditingLifecycleRules were fully scaffolded but nothing called
    // them. Mirrors Compliance.Permits' Approve/Reject/Complete/Expire/Close/Archive build-out
    // shape.

    /// <summary>
    /// Starts the audit program/execution (Created -&gt; Active). Valid only from
    /// <see cref="AuditingStatus.Created"/>.
    /// </summary>
    public void Start(
        UserId startedBy,
        UtcTimestamp startedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AuditingLifecycleRules.CanActivate, AuditingStatus.Created);

        Status = AuditingStatus.Active;

        AddDomainEvent(new AuditingStartedEvent(
            EventId: new AuditingEventId(Uuid7.New()),
            AuditingId: AuditingId,
            StartedBy: startedBy,
            StartedAtUtc: startedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Records the audit's findings and computed compliance score, marking the audit completed
    /// (Active -&gt; Completed). Valid only from <see cref="AuditingStatus.Active"/>.
    /// </summary>
    public void RecordFindings(
        UserId completedBy,
        int findingsCount,
        decimal? score,
        string complianceRating,
        UtcTimestamp completedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AuditingLifecycleRules.CanComplete, AuditingStatus.Active);
        if (findingsCount < 0)
            throw new ArgumentOutOfRangeException(nameof(findingsCount), "Findings count must not be negative.");

        FindingsCount = findingsCount;
        Score = score;
        ComplianceRating = (complianceRating ?? "NotRated").Trim();
        CompletedAtUtc = completedAtUtc;
        Status = AuditingStatus.Completed;

        AddDomainEvent(new AuditingFindingsRecordedEvent(
            EventId: new AuditingEventId(Uuid7.New()),
            AuditingId: AuditingId,
            CompletedBy: completedBy,
            FindingsCount: findingsCount,
            Score: score,
            ComplianceRating: ComplianceRating,
            CompletedAtUtc: completedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Formally closes a completed audit (Completed -&gt; Closed). Valid only from
    /// <see cref="AuditingStatus.Completed"/>.
    /// </summary>
    public void Close(
        UserId closedBy,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AuditingLifecycleRules.CanClose, AuditingStatus.Completed);

        Status = AuditingStatus.Closed;

        AddDomainEvent(new AuditingClosedEvent(
            EventId: new AuditingEventId(Uuid7.New()),
            AuditingId: AuditingId,
            ClosedBy: closedBy,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Archives a closed audit for compliance retention (Closed -&gt; Archived). Valid only from
    /// <see cref="AuditingStatus.Closed"/>.
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, AuditingLifecycleRules.CanArchive, AuditingStatus.Closed);

        Status = AuditingStatus.Archived;

        AddDomainEvent(new AuditingArchivedEvent(
            EventId: new AuditingEventId(Uuid7.New()),
            AuditingId: AuditingId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IAuditingDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params AuditingStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Auditing {AuditingId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<AuditingStatus, bool> guard, AuditingStatus expected)
    {
        if (guard(Status)) return;

        throw new AuditingInvalidStatusTransitionException(
            AuditingId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
