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
}
