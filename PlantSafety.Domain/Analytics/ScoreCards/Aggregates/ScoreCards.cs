// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Domain
//  File:           ScoreCards.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  scorecard record shape grounded in project_plantsafety_domains.md
//                  Module 17 (Score Cards: KPI scoring, performance index
//                  computation, executive reporting).
//                  Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the ScoreCards bounded context. Tracks a single
//                  generated scorecard: its reporting period, KPI category, computed
//                  performance index, and generation scope.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.ScoreCards.Aggregates;

/// <summary>Aggregate root for the ScoreCards bounded context.</summary>
public sealed partial class ScoreCards
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this ScoreCards aggregate.</summary>
    public ScoreCardsId ScoreCardsId { get; private set; }

    /// <summary>Tenant that owns this scorecard record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this scorecard is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ScoreCardsStatus Status { get; private set; }

    // ---- Scorecard-specific business properties -------------------------------------

    /// <summary>Human-readable scorecard name.</summary>
    public string ScoreCardName { get; private set; } = string.Empty;

    /// <summary>Reporting period this scorecard covers (e.g., "2026-Q3").</summary>
    public string ReportingPeriod { get; private set; } = string.Empty;

    /// <summary>KPI category this scorecard reports on (e.g., Safety, Compliance, Operations).</summary>
    public string KpiCategory { get; private set; } = string.Empty;

    /// <summary>Computed performance index (0-100).</summary>
    public decimal PerformanceIndex { get; private set; }

    /// <summary>Site this scorecard was generated for, if site-scoped. Null means enterprise-wide.</summary>
    public Guid? GeneratedForSiteId { get; private set; }

    /// <summary>UTC timestamp this scorecard was generated.</summary>
    public UtcTimestamp GeneratedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Lifecycle attributes (Publish/Finalize/Close/Archive) --------------

    /// <summary>Identity of the user who published this scorecard, if published.</summary>
    public UserId? PublishedByUserId { get; private set; }

    /// <summary>UTC instant the scorecard was published.</summary>
    public UtcTimestamp? PublishedAtUtc { get; private set; }

    /// <summary>Identity of the user who finalized this scorecard, if finalized.</summary>
    public UserId? FinalizedByUserId { get; private set; }

    /// <summary>UTC instant the scorecard was finalized.</summary>
    public UtcTimestamp? FinalizedAtUtc { get; private set; }

    /// <summary>Identity of the user who closed this scorecard, if closed.</summary>
    public UserId? ClosedByUserId { get; private set; }

    /// <summary>UTC instant the scorecard was closed.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    /// <summary>UTC instant the scorecard was archived for long-term retention.</summary>
    public UtcTimestamp? ArchivedAtUtc { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IScoreCardsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IScoreCardsDomainEvent> DomainEvents
        => new ReadOnlyCollection<IScoreCardsDomainEvent>(_domainEvents);

    // For rehydration only.
    private ScoreCards() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new ScoreCards aggregate and raises <see cref="ScoreCardsCreatedEvent"/>.</summary>
    public static ScoreCards Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string scoreCardName = "",
        string reportingPeriod = "",
        string kpiCategory = "",
        decimal performanceIndex = 0m,
        Guid? generatedForSiteId = null)
    {
        var agg = new ScoreCards
        {
            ScoreCardsId          = ScoreCardsId.New(),
            TenantId              = tenantId,
            SiteId                = siteId,
            Status                = ScoreCardsStatus.Created,
            ScoreCardName         = (scoreCardName ?? string.Empty).Trim(),
            ReportingPeriod       = (reportingPeriod ?? string.Empty).Trim(),
            KpiCategory           = (kpiCategory ?? string.Empty).Trim(),
            PerformanceIndex      = performanceIndex,
            GeneratedForSiteId    = generatedForSiteId,
            GeneratedAtUtc        = createdAtUtc,
            CreatedAtUtc          = createdAtUtc,
            CreatedBy             = createdBy,
        };

        agg.AddDomainEvent(new ScoreCardsCreatedEvent(
            EventId:       new ScoreCardsEventId(Uuid7.New()),
            ScoreCardsId:      agg.ScoreCardsId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: Publish / Finalize --------------------------------------

    /// <summary>
    /// Publishes the scorecard for consumption (Created -&gt; Active). Valid only from
    /// <see cref="ScoreCardsStatus.Created"/>.
    /// </summary>
    public void Publish(
        UserId publishedBy,
        UtcTimestamp publishedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ScoreCardsLifecycleRules.CanActivate, ScoreCardsStatus.Created);

        PublishedByUserId = publishedBy;
        PublishedAtUtc = publishedAtUtc;
        Status = ScoreCardsStatus.Active;

        AddDomainEvent(new ScoreCardsPublishedEvent(
            EventId: new ScoreCardsEventId(Uuid7.New()),
            ScoreCardsId: ScoreCardsId,
            PublishedByUserId: publishedBy,
            PublishedAtUtc: publishedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Finalizes a published scorecard (Active -&gt; Completed). Valid only from
    /// <see cref="ScoreCardsStatus.Active"/>.
    /// </summary>
    public void Finalize(
        UserId finalizedBy,
        UtcTimestamp finalizedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ScoreCardsLifecycleRules.CanComplete, ScoreCardsStatus.Active);

        FinalizedByUserId = finalizedBy;
        FinalizedAtUtc = finalizedAtUtc;
        Status = ScoreCardsStatus.Completed;

        AddDomainEvent(new ScoreCardsFinalizedEvent(
            EventId: new ScoreCardsEventId(Uuid7.New()),
            ScoreCardsId: ScoreCardsId,
            FinalizedByUserId: finalizedBy,
            FinalizedAtUtc: finalizedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Close / Archive -----------------------------------------

    /// <summary>
    /// Formally closes a finalized scorecard (Completed -&gt; Closed). Valid only from
    /// <see cref="ScoreCardsStatus.Completed"/>.
    /// </summary>
    public void Close(
        UserId closedBy,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ScoreCardsLifecycleRules.CanClose, ScoreCardsStatus.Completed);

        ClosedByUserId = closedBy;
        ClosedAtUtc = closedAtUtc;
        Status = ScoreCardsStatus.Closed;

        AddDomainEvent(new ScoreCardsClosedEvent(
            EventId: new ScoreCardsEventId(Uuid7.New()),
            ScoreCardsId: ScoreCardsId,
            ClosedByUserId: closedBy,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Archives a closed scorecard for long-term retention (Closed -&gt; Archived). Valid only from
    /// <see cref="ScoreCardsStatus.Closed"/>.
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ScoreCardsLifecycleRules.CanArchive, ScoreCardsStatus.Closed);

        ArchivedAtUtc = archivedAtUtc;
        Status = ScoreCardsStatus.Archived;

        AddDomainEvent(new ScoreCardsArchivedEvent(
            EventId: new ScoreCardsEventId(Uuid7.New()),
            ScoreCardsId: ScoreCardsId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IScoreCardsDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ScoreCardsStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for ScoreCards {ScoreCardsId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<ScoreCardsStatus, bool> guard, ScoreCardsStatus expected)
    {
        if (guard(Status)) return;

        throw new ScoreCardsInvalidStatusTransitionException(
            ScoreCardsId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
