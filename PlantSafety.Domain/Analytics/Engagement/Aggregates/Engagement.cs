// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Domain
//  File:           Engagement.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  worker-engagement record shape. NOTE: no dedicated Engagement
//                  field-level spec was found in project_plantsafety_domains.md (v4.0
//                  folds engagement score into Module 7 Culture's KPI list);
//                  project_plantsafety_overview.md Section 5 confirms "Engagement
//                  Analytics" (4.39) as a distinct Analytics sub-module. Properties
//                  below use the same engagement-score concept as the sibling Culture
//                  aggregate, scoped per-worker rather than per-survey-period.
//                  Flagged as partially-unsourced.
//                  Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Engagement bounded context. Tracks a single
//                  worker engagement measurement: its score, survey period,
//                  participation channel, and response volume.
//                  Additive-only evolution.
//  UPDATED:        2026-08-02 -- Phase 4 full build-out: added Review/RecordTouchpoint/Complete/
//                  Close/Archive lifecycle methods. Previously this aggregate had only a Create
//                  factory -- EngagementStatus/EngagementLifecycleRules were fully scaffolded but
//                  nothing called them, and no file anywhere under
//                  PlantSafety.Application\Analytics\Engagement referenced this aggregate at all
//                  (that Application folder did not exist). Mirrors Permits.cs's lifecycle
//                  shape and guard/event/exception conventions.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Engagement.Aggregates;

/// <summary>Aggregate root for the Engagement bounded context.</summary>
public sealed partial class Engagement
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Engagement aggregate.</summary>
    public EngagementId EngagementId { get; private set; }

    /// <summary>Tenant that owns this engagement record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this engagement measurement is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public EngagementStatus Status { get; private set; }

    // ---- Engagement-specific business properties -------------------------------------

    /// <summary>Worker this engagement measurement relates to.</summary>
    public UserId WorkerId { get; private set; }

    /// <summary>Computed engagement score (0-100).</summary>
    public decimal EngagementScore { get; private set; }

    /// <summary>Survey period this measurement covers (e.g., "2026-Q3").</summary>
    public string SurveyPeriod { get; private set; } = string.Empty;

    /// <summary>Channel through which engagement was captured (e.g., MobileApp, Kiosk, Paper, Interview).</summary>
    public string ParticipationChannel { get; private set; } = string.Empty;

    /// <summary>Number of responses/interactions captured for this measurement.</summary>
    public int ResponseCount { get; private set; }

    /// <summary>UTC timestamp this worker last engaged with a safety touchpoint.</summary>
    public UtcTimestamp? LastEngagedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IEngagementDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IEngagementDomainEvent> DomainEvents
        => new ReadOnlyCollection<IEngagementDomainEvent>(_domainEvents);

    // For rehydration only.
    private Engagement() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Engagement aggregate and raises <see cref="EngagementCreatedEvent"/>.</summary>
    public static Engagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        UserId workerId = default,
        decimal engagementScore = 0m,
        string surveyPeriod = "",
        string participationChannel = "",
        int responseCount = 0,
        UtcTimestamp? lastEngagedAtUtc = null)
    {
        var agg = new Engagement
        {
            EngagementId            = EngagementId.New(),
            TenantId                = tenantId,
            SiteId                  = siteId,
            Status                  = EngagementStatus.Created,
            WorkerId                = workerId,
            EngagementScore         = engagementScore,
            SurveyPeriod            = (surveyPeriod ?? string.Empty).Trim(),
            ParticipationChannel    = (participationChannel ?? string.Empty).Trim(),
            ResponseCount           = responseCount < 0 ? 0 : responseCount,
            LastEngagedAtUtc        = lastEngagedAtUtc,
            CreatedAtUtc            = createdAtUtc,
            CreatedBy               = createdBy,
        };

        agg.AddDomainEvent(new EngagementCreatedEvent(
            EventId:       new EngagementEventId(Uuid7.New()),
            EngagementId:      agg.EngagementId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: Review / RecordTouchpoint --------------------------------

    /// <summary>
    /// Reviews and activates a newly created Engagement measurement (Created -&gt; Active). Valid
    /// only from <see cref="EngagementStatus.Created"/>.
    /// </summary>
    public void Review(
        UserId reviewedBy,
        UtcTimestamp reviewedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EngagementLifecycleRules.CanActivate, EngagementStatus.Created);

        Status = EngagementStatus.Active;

        AddDomainEvent(new EngagementReviewedEvent(
            EventId: new EngagementEventId(Uuid7.New()),
            EngagementId: EngagementId,
            ReviewedByUserId: reviewedBy,
            ReviewedAtUtc: reviewedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Records a worker touchpoint against an active Engagement measurement, updating
    /// <see cref="LastEngagedAtUtc"/> and <see cref="ResponseCount"/>. Does not change
    /// <see cref="Status"/>. Valid only from <see cref="EngagementStatus.Active"/>.
    /// </summary>
    public void RecordTouchpoint(
        UtcTimestamp touchedAtUtc,
        int responseCount,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EngagementLifecycleRules.CanRecordTouchpoint, EngagementStatus.Active);
        if (responseCount < 0)
            throw new ArgumentException("Response count must not be negative.", nameof(responseCount));

        LastEngagedAtUtc = touchedAtUtc;
        ResponseCount = responseCount;

        AddDomainEvent(new EngagementTouchpointRecordedEvent(
            EventId: new EngagementEventId(Uuid7.New()),
            EngagementId: EngagementId,
            TouchedAtUtc: touchedAtUtc,
            ResponseCount: responseCount,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Complete / Close / Archive -------------------------------

    /// <summary>
    /// Completes an active Engagement measurement (Active -&gt; Completed). Valid only from
    /// <see cref="EngagementStatus.Active"/>.
    /// </summary>
    public void Complete(
        UserId completedBy,
        UtcTimestamp completedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EngagementLifecycleRules.CanComplete, EngagementStatus.Active);

        Status = EngagementStatus.Completed;

        AddDomainEvent(new EngagementCompletedEvent(
            EventId: new EngagementEventId(Uuid7.New()),
            EngagementId: EngagementId,
            CompletedByUserId: completedBy,
            CompletedAtUtc: completedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Formally closes a completed Engagement measurement (Completed -&gt; Closed). Valid only from
    /// <see cref="EngagementStatus.Completed"/>.
    /// </summary>
    public void Close(
        UserId closedBy,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EngagementLifecycleRules.CanClose, EngagementStatus.Completed);

        Status = EngagementStatus.Closed;

        AddDomainEvent(new EngagementClosedEvent(
            EventId: new EngagementEventId(Uuid7.New()),
            EngagementId: EngagementId,
            ClosedByUserId: closedBy,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Archives a closed Engagement measurement for long-term retention (Closed -&gt; Archived).
    /// Valid only from <see cref="EngagementStatus.Closed"/>. See
    /// <see cref="Policies.EngagementArchivalPolicy"/> for eligibility outside the aggregate
    /// boundary (e.g. batch archival jobs).
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, EngagementLifecycleRules.CanArchive, EngagementStatus.Closed);

        Status = EngagementStatus.Archived;

        AddDomainEvent(new EngagementArchivedEvent(
            EventId: new EngagementEventId(Uuid7.New()),
            EngagementId: EngagementId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IEngagementDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params EngagementStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Engagement {EngagementId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<EngagementStatus, bool> guard, EngagementStatus expected)
    {
        if (guard(Status)) return;

        throw new EngagementInvalidStatusTransitionException(
            EngagementId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
