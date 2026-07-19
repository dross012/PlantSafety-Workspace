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
}
