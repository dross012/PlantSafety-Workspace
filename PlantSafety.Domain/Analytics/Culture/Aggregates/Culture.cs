// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Domain
//  File:           Culture.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  safety-culture measurement record shape grounded in
//                  project_plantsafety_domains.md Module 7 (Culture: safety culture
//                  measurement, modeling, and analytics; KPIs: culture index,
//                  engagement score).
//                  Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Culture bounded context. Tracks a single
//                  safety-culture survey period: its computed culture index,
//                  engagement score, participation rate, and top risk theme.
//                  Additive-only evolution.
//  UPDATED:        2026-08-02 -- Phase 4 full build-out: added Review/Publish/Close/Archive
//                  lifecycle methods. Previously this aggregate had only a Create factory --
//                  CultureStatus/CultureLifecycleRules/CultureArchivalPolicy were fully scaffolded
//                  but nothing called them. Mirrors Compliance.Permits' Approve/Complete/Close/
//                  Archive lifecycle shape and guard/event/exception conventions.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Culture.Aggregates;

/// <summary>Aggregate root for the Culture bounded context.</summary>
public sealed partial class Culture
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Culture aggregate.</summary>
    public CultureId CultureId { get; private set; }

    /// <summary>Tenant that owns this culture record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this culture measurement is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public CultureStatus Status { get; private set; }

    // ---- Culture-specific business properties -------------------------------------

    /// <summary>Survey period this measurement covers (e.g., "2026-Q3").</summary>
    public string SurveyPeriod { get; private set; } = string.Empty;

    /// <summary>Computed safety culture index score (0-100).</summary>
    public decimal CultureIndexScore { get; private set; }

    /// <summary>Computed engagement score (0-100).</summary>
    public decimal EngagementScore { get; private set; }

    /// <summary>Survey participation rate (0-100).</summary>
    public decimal ParticipationRate { get; private set; }

    /// <summary>Top risk theme identified from this survey period.</summary>
    public string TopRiskTheme { get; private set; } = string.Empty;

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Lifecycle attributes (Review/Publish/Close/Archive) ----------------

    /// <summary>Identity of the user who reviewed this survey period, if reviewed.</summary>
    public UserId? ReviewedByUserId { get; private set; }

    /// <summary>UTC instant the survey period was reviewed (Created -&gt; Active).</summary>
    public UtcTimestamp? ReviewedAtUtc { get; private set; }

    /// <summary>Identity of the user who published this survey period, if published.</summary>
    public UserId? PublishedByUserId { get; private set; }

    /// <summary>UTC instant the survey period was published (Active -&gt; Completed).</summary>
    public UtcTimestamp? PublishedAtUtc { get; private set; }

    /// <summary>Identity of the user who closed this survey period, if closed.</summary>
    public UserId? ClosedByUserId { get; private set; }

    /// <summary>UTC instant the survey period was closed (Completed -&gt; Closed).</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    /// <summary>UTC instant the survey period was archived for long-term retention (Closed -&gt; Archived).</summary>
    public UtcTimestamp? ArchivedAtUtc { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ICultureDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ICultureDomainEvent> DomainEvents
        => new ReadOnlyCollection<ICultureDomainEvent>(_domainEvents);

    // For rehydration only.
    private Culture() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Culture aggregate and raises <see cref="CultureCreatedEvent"/>.</summary>
    public static Culture Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string surveyPeriod = "",
        decimal cultureIndexScore = 0m,
        decimal engagementScore = 0m,
        decimal participationRate = 0m,
        string topRiskTheme = "")
    {
        var agg = new Culture
        {
            CultureId             = CultureId.New(),
            TenantId              = tenantId,
            SiteId                = siteId,
            Status                = CultureStatus.Created,
            SurveyPeriod          = (surveyPeriod ?? string.Empty).Trim(),
            CultureIndexScore     = cultureIndexScore,
            EngagementScore       = engagementScore,
            ParticipationRate     = participationRate,
            TopRiskTheme          = (topRiskTheme ?? string.Empty).Trim(),
            CreatedAtUtc          = createdAtUtc,
            CreatedBy             = createdBy,
        };

        agg.AddDomainEvent(new CultureCreatedEvent(
            EventId:       new CultureEventId(Uuid7.New()),
            CultureId:      agg.CultureId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: Review / Publish / Close / Archive ----------------------

    /// <summary>
    /// Reviews a submitted survey period (Created -&gt; Active). Valid only from
    /// <see cref="CultureStatus.Created"/>.
    /// </summary>
    public void Review(
        UserId reviewedBy,
        UtcTimestamp reviewedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, CultureLifecycleRules.CanActivate, CultureStatus.Created);

        ReviewedByUserId = reviewedBy;
        ReviewedAtUtc = reviewedAtUtc;
        Status = CultureStatus.Active;

        AddDomainEvent(new CultureReviewedEvent(
            EventId: new CultureEventId(Uuid7.New()),
            CultureId: CultureId,
            ReviewedByUserId: reviewedBy,
            ReviewedAtUtc: reviewedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Publishes a reviewed survey period's results (Active -&gt; Completed). Valid only from
    /// <see cref="CultureStatus.Active"/>.
    /// </summary>
    public void Publish(
        UserId publishedBy,
        UtcTimestamp publishedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, CultureLifecycleRules.CanComplete, CultureStatus.Active);

        PublishedByUserId = publishedBy;
        PublishedAtUtc = publishedAtUtc;
        Status = CultureStatus.Completed;

        AddDomainEvent(new CulturePublishedEvent(
            EventId: new CultureEventId(Uuid7.New()),
            CultureId: CultureId,
            PublishedByUserId: publishedBy,
            PublishedAtUtc: publishedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Formally closes a published survey period (Completed -&gt; Closed). Valid only from
    /// <see cref="CultureStatus.Completed"/>.
    /// </summary>
    public void Close(
        UserId closedBy,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, CultureLifecycleRules.CanClose, CultureStatus.Completed);

        ClosedByUserId = closedBy;
        ClosedAtUtc = closedAtUtc;
        Status = CultureStatus.Closed;

        AddDomainEvent(new CultureClosedEvent(
            EventId: new CultureEventId(Uuid7.New()),
            CultureId: CultureId,
            ClosedByUserId: closedBy,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Archives a closed survey period for long-term retention (Closed -&gt; Archived). Valid only
    /// from <see cref="CultureStatus.Closed"/>. See <see cref="Policies.CultureArchivalPolicy"/>
    /// for eligibility outside the aggregate boundary (e.g. batch archival jobs).
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, CultureLifecycleRules.CanArchive, CultureStatus.Closed);

        ArchivedAtUtc = archivedAtUtc;
        Status = CultureStatus.Archived;

        AddDomainEvent(new CultureArchivedEvent(
            EventId: new CultureEventId(Uuid7.New()),
            CultureId: CultureId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ICultureDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params CultureStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Culture {CultureId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<CultureStatus, bool> guard, CultureStatus expected)
    {
        if (guard(Status)) return;

        throw new CultureInvalidStatusTransitionException(
            CultureId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
