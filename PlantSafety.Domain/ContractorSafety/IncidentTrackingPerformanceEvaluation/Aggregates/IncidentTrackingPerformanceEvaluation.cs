// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
//  Layer:          Domain
//  File:           IncidentTrackingPerformanceEvaluation.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single contractor incident tracking and performance evaluation
//                  record. Includes contractors in the facility incident process: investigation,
//                  root-cause analysis, CAPA documentation, and eligibility scoring for future work.
//                  Models the real lifecycle: reporting, investigation, CAPA documentation, and
//                  eligibility determination.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Aggregates;

/// <summary>Aggregate root for a single contractor incident tracking and performance evaluation record.</summary>
public sealed partial class IncidentTrackingPerformanceEvaluation
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this IncidentTrackingPerformanceEvaluation aggregate.</summary>
    public IncidentTrackingPerformanceEvaluationId IncidentTrackingPerformanceEvaluationId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public IncidentTrackingPerformanceEvaluationStatus Status { get; private set; }

    // ---- Evaluation identity ---------------------------------------------------

    /// <summary>Human-readable title of this evaluation.</summary>
    public string EvaluationTitle { get; private set; } = string.Empty;

    /// <summary>Incident category.</summary>
    public ContractorIncidentCategory IncidentCategory { get; private set; }

    /// <summary>Contractor company name involved.</summary>
    public string ContractorName { get; private set; } = string.Empty;

    /// <summary>Identity of the employee performing the evaluation.</summary>
    public UserId EvaluatedByUserId { get; private set; }

    /// <summary>UTC timestamp the investigation was completed, if any.</summary>
    public UtcTimestamp? InvestigatedAtUtc { get; private set; }

    /// <summary>Narrative root-cause summary.</summary>
    public string? RootCauseSummary { get; private set; }

    /// <summary>Number of CAPA (corrective/preventive action) items documented.</summary>
    public int CapaCount { get; private set; }

    /// <summary>Overall contractor safety performance score (0-100).</summary>
    public decimal? PerformanceScore { get; private set; }

    /// <summary>UTC timestamp the eligibility determination was closed, if any.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IIncidentTrackingPerformanceEvaluationDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IIncidentTrackingPerformanceEvaluationDomainEvent> DomainEvents
        => new ReadOnlyCollection<IIncidentTrackingPerformanceEvaluationDomainEvent>(_domainEvents);

    // For rehydration only.
    private IncidentTrackingPerformanceEvaluation() { }

    // ---- Factory: Create (Report) -----------------------------------------------------------

    /// <summary>Reports a new contractor incident/near-miss/unsafe condition and raises <see cref="ContractorIncidentReportedEvent"/>.</summary>
    public static IncidentTrackingPerformanceEvaluation Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string evaluationTitle = "",
        ContractorIncidentCategory incidentCategory = ContractorIncidentCategory.Incident,
        string contractorName = "")
    {
        var agg = new IncidentTrackingPerformanceEvaluation
        {
            IncidentTrackingPerformanceEvaluationId = IncidentTrackingPerformanceEvaluationId.New(),
            TenantId          = tenantId,
            SiteId            = siteId,
            Status            = IncidentTrackingPerformanceEvaluationStatus.Reported,
            EvaluationTitle   = (evaluationTitle ?? string.Empty).Trim(),
            IncidentCategory  = incidentCategory,
            ContractorName    = (contractorName ?? string.Empty).Trim(),
            CreatedAtUtc      = createdAtUtc,
            CreatedBy         = createdBy,
        };

        agg.AddDomainEvent(new ContractorIncidentReportedEvent(
            IncidentTrackingPerformanceEvaluationEventId.New(), agg.IncidentTrackingPerformanceEvaluationId, incidentCategory,
            createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: BeginInvestigation -----------------------------------------------

    /// <summary>Begins the investigation and root-cause analysis.</summary>
    public void BeginInvestigation(UserId evaluatedByUserId, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(IncidentTrackingPerformanceEvaluationStatus.Reported);

        EvaluatedByUserId = evaluatedByUserId;
        Status = IncidentTrackingPerformanceEvaluationStatus.InvestigationInProgress;

        AddDomainEvent(new ContractorIncidentInvestigationStartedEvent(
            IncidentTrackingPerformanceEvaluationEventId.New(), IncidentTrackingPerformanceEvaluationId, evaluatedByUserId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: DocumentCapa -----------------------------------------------

    /// <summary>Documents corrective and preventive actions (CAPA) following the investigation.</summary>
    public void DocumentCapa(int capaCount, string rootCauseSummary, UtcTimestamp investigatedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(IncidentTrackingPerformanceEvaluationStatus.InvestigationInProgress);

        if (capaCount < 0)
            throw new ArgumentOutOfRangeException(nameof(capaCount), "CAPA count cannot be negative.");

        CapaCount         = capaCount;
        RootCauseSummary  = rootCauseSummary;
        InvestigatedAtUtc = investigatedAtUtc;
        Status = IncidentTrackingPerformanceEvaluationStatus.CapaDocumented;

        AddDomainEvent(new ContractorIncidentCapaDocumentedEvent(
            IncidentTrackingPerformanceEvaluationEventId.New(), IncidentTrackingPerformanceEvaluationId, capaCount, investigatedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: DetermineEligibility -----------------------------------------------

    /// <summary>Determines the contractor's eligibility for future work based on overall safety performance.</summary>
    public void DetermineEligibility(decimal performanceScore, UtcTimestamp closedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(IncidentTrackingPerformanceEvaluationStatus.CapaDocumented);

        if (performanceScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(performanceScore), "Performance score must be between 0 and 100.");

        PerformanceScore = performanceScore;
        ClosedAtUtc = closedAtUtc;

        if (performanceScore >= 70m)
        {
            Status = IncidentTrackingPerformanceEvaluationStatus.EligibilityApproved;
            AddDomainEvent(new ContractorEligibilityApprovedEvent(
                IncidentTrackingPerformanceEvaluationEventId.New(), IncidentTrackingPerformanceEvaluationId, performanceScore, closedAtUtc, actor, correlationId, causationId));
        }
        else
        {
            Status = IncidentTrackingPerformanceEvaluationStatus.EligibilityRestricted;
            AddDomainEvent(new ContractorEligibilityRestrictedEvent(
                IncidentTrackingPerformanceEvaluationEventId.New(), IncidentTrackingPerformanceEvaluationId, performanceScore, closedAtUtc, actor, correlationId, causationId));
        }
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IIncidentTrackingPerformanceEvaluationDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params IncidentTrackingPerformanceEvaluationStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for IncidentTrackingPerformanceEvaluation {IncidentTrackingPerformanceEvaluationId}.");
    }
}
