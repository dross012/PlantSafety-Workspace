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
}
