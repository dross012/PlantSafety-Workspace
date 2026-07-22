// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment
//  Layer:          Domain
//  File:           CultureAssessment.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root representing a periodic safety-culture health assessment: leadership engagement, reporting culture, and behavioral metrics rolled up into an overall culture score for a tenant/site over a defined period.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.CultureAssessment.Aggregates;

/// <summary>Aggregate root for the CultureAssessment bounded context.</summary>
public sealed partial class CultureAssessment
{
    // ---- Identity ------------------------------------------------------

    /// <summary>Strong-typed identity of this CultureAssessment aggregate.</summary>
    public CultureAssessmentId CultureAssessmentId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public CultureAssessmentStatus Status { get; private set; }

    // ---- Business properties --------------------------------------------
    /// <summary>Human-readable assessment title (e.g. "Q3 2026 Safety Culture Assessment").</summary>
    public string AssessmentTitle { get; private set; } = string.Empty;

    /// <summary>UTC start of the period this assessment covers.</summary>
    public UtcTimestamp PeriodStartUtc { get; private set; }

    /// <summary>UTC end of the period this assessment covers.</summary>
    public UtcTimestamp PeriodEndUtc { get; private set; }

    /// <summary>Overall computed culture health score (0-100), when available.</summary>
    public decimal? OverallCultureScore { get; private set; }

    /// <summary>Leadership engagement/participation score (0-100), when available.</summary>
    public decimal? LeadershipEngagementScore { get; private set; }

    /// <summary>Narrative summary of assessment findings and themes.</summary>
    public string Summary { get; private set; } = string.Empty;

    /// <summary>
    /// Optional cross-module reference (by StrongId, not a compiled dependency) to a
    /// RiskManagement hazard/RCA record this assessment's culture findings were sourced from.
    /// Per PlantSafety Program Structure "Safety Culture Module" §9 (Integration with Other
    /// Modules): "Risk Management → hazard reporting, RCA, FMEA".
    /// </summary>
    public Guid? RelatedRiskManagementHazardId { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>UTC instant the finalized report was published, once published.</summary>
    public UtcTimestamp? PublishedAtUtc { get; private set; }

    // ---- Domain Events ---------------------------------------------------

    private readonly List<ICultureAssessmentDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ICultureAssessmentDomainEvent> DomainEvents
        => new ReadOnlyCollection<ICultureAssessmentDomainEvent>(_domainEvents);

    // For rehydration only.
    private CultureAssessment() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new CultureAssessment aggregate and raises <see cref="CultureAssessmentCreatedEvent"/>.</summary>
    /// <param name="createdBy">CRUD authorship identity.</param>
    /// <param name="createdAtUtc">Creation timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="siteId">Optional owning site.</param>
    public static CultureAssessment Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string assessmentTitle = "",
        UtcTimestamp periodStartUtc = default,
        UtcTimestamp periodEndUtc = default,
        decimal? overallCultureScore = null,
        decimal? leadershipEngagementScore = null,
        string summary = "",
        Guid? relatedRiskManagementHazardId = null)
    {
        var agg = new CultureAssessment
        {
            CultureAssessmentId      = CultureAssessmentId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = CultureAssessmentStatus.Created,
            AssessmentTitle           = (assessmentTitle ?? string.Empty).Trim(),
            PeriodStartUtc            = periodStartUtc,
            PeriodEndUtc              = periodEndUtc,
            OverallCultureScore       = overallCultureScore,
            LeadershipEngagementScore = leadershipEngagementScore,
            Summary                   = (summary ?? string.Empty).Trim(),
            RelatedRiskManagementHazardId = relatedRiskManagementHazardId,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new CultureAssessmentCreatedEvent(
            EventId:       new CultureAssessmentEventId(Uuid7.New()),
            CultureAssessmentId:      agg.CultureAssessmentId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle behavior --------------------------------------------------

    /// <summary>
    /// Records/recomputes the overall culture health score and leadership engagement score for
    /// this assessment period, typically rolled up from the eight canonical
    /// <see cref="SmartEnums.CultureMetricType"/> KPIs (near-miss reporting rate, stop-work
    /// actions, PPE compliance rate, behavioral observation completion, leadership engagement
    /// frequency, training completion rate, safety meeting participation, safety suggestion
    /// submissions). (Created → Active).
    /// </summary>
    /// <exception cref="CultureAssessmentInvalidStatusTransitionException">Thrown when the assessment has already been published or archived.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a score is outside the 0-100 range.</exception>
    public void RecordScore(
        decimal overallCultureScore,
        decimal? leadershipEngagementScore,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        UtcTimestamp occurredAtUtc)
    {
        EnsureStatusIsOneOf(CultureAssessmentStatus.Created, CultureAssessmentStatus.Active);

        if (overallCultureScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(overallCultureScore), overallCultureScore, "OverallCultureScore must be between 0 and 100.");
        if (leadershipEngagementScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(leadershipEngagementScore), leadershipEngagementScore, "LeadershipEngagementScore must be between 0 and 100.");

        OverallCultureScore = overallCultureScore;
        LeadershipEngagementScore = leadershipEngagementScore;
        Status = CultureAssessmentStatus.Active;

        AddDomainEvent(new CultureAssessmentScoreRecordedEvent(
            EventId: new CultureAssessmentEventId(Uuid7.New()),
            CultureAssessmentId: CultureAssessmentId,
            OverallCultureScore: overallCultureScore,
            LeadershipEngagementScore: leadershipEngagementScore,
            RecordedAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Publishes the finalized assessment report to Scorecards and executive dashboards
    /// (Active → Completed). Requires a score to have already been recorded.
    /// </summary>
    /// <exception cref="CultureAssessmentInvalidStatusTransitionException">Thrown when the assessment is not in an active state.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no score has been recorded yet.</exception>
    public void PublishReport(
        string summary,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        UtcTimestamp occurredAtUtc)
    {
        if (Status != CultureAssessmentStatus.Active)
            throw new CultureAssessmentInvalidStatusTransitionException(
                CultureAssessmentId, Status.ToString(), CultureAssessmentStatus.Completed.ToString(), correlationId, causationId);

        if (OverallCultureScore is null)
            throw new InvalidOperationException(
                "Cannot publish CultureAssessment " + CultureAssessmentId + " report before a score has been recorded.");

        Summary = (summary ?? Summary).Trim();
        PublishedAtUtc = occurredAtUtc;
        Status = CultureAssessmentStatus.Completed;

        AddDomainEvent(new CultureAssessmentReportPublishedEvent(
            EventId: new CultureAssessmentEventId(Uuid7.New()),
            CultureAssessmentId: CultureAssessmentId,
            OverallCultureScore: OverallCultureScore,
            PublishedAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ICultureAssessmentDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params CultureAssessmentStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            "Invalid lifecycle transition from '" + Status + "' for CultureAssessment " + CultureAssessmentId + ".");
    }
}