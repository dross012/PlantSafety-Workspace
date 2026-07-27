// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           Risk.cs
//  Created:        2026-07-20
//  Updated:        2026-07-21 -- replaced the generic single-Create() placeholder with the real
//                  3-phase risk management process documented in "Full Risk Management Process
//                  (Detailed & Structured)": Phase 1 Risk Assessment (identify/score/prioritize/
//                  document), Phase 2 Risk Mitigation (apply Hierarchy-of-Controls measures/
//                  validate effectiveness), Phase 3 Risk Reporting & Review (periodic
//                  monitoring/review). Control measures are modeled as a related child entity
//                  (see Entities/ControlMeasure.cs) because the source document treats each
//                  applied control as a distinct tracked item with its own responsible person,
//                  hierarchy level, and justification requirement.
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the RiskManagement bounded context. Tracks a single
//                  identified hazard from identification through scoring, prioritization,
//                  control-measure mitigation, effectiveness validation, and periodic review.
//                  Additive-only evolution.
// ============================================================================
using System.Collections.ObjectModel;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Common.Abstractions;
using PlantSafety.Domain.RiskManagement.Entities;
using PlantSafety.Domain.RiskManagement.Enums;
using PlantSafety.Domain.RiskManagement.Events;
using PlantSafety.Domain.RiskManagement.Exceptions;
using PlantSafety.Domain.RiskManagement.ValueObjects;
using PlantSafety.Domain.RiskManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.RiskManagement.Aggregates;

/// <summary>Aggregate root for the RiskManagement bounded context.</summary>
public sealed partial class Risk
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Risk aggregate.</summary>
    public RiskId RiskId { get; private set; }

    /// <summary>Tenant that owns this risk record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this risk is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status within the 3-phase risk management process.</summary>
    public RiskStatus Status { get; private set; }

    // ---- Hazard identification (Phase 1, Step 1) --------------------------------

    /// <summary>Human-readable hazard title/summary.</summary>
    public string RiskTitle { get; private set; } = string.Empty;

    /// <summary>Qualitative risk level, derived from the most recent RiskScore (Low/Medium/High/Critical).</summary>
    public RiskLevel Category { get; private set; } = RiskLevel.Default;

    /// <summary>Free-form location description (building/zone/equipment) where the hazard exists.</summary>
    public string Location { get; private set; } = string.Empty;

    /// <summary>Hazard source description (mechanical, electrical, chemical, ergonomic, process, human factors).</summary>
    public string HazardSource { get; private set; } = string.Empty;

    /// <summary>Identity of the accountable owner.</summary>
    public UserId OwnerId { get; private set; }

    // ---- Assessment (Phase 1, Steps 2-4) -----------------------------------------

    /// <summary>Most recently recorded Severity x Likelihood risk score, once assessed.</summary>
    public RiskScore? CurrentScore { get; private set; }

    /// <summary>Assigned P1-P4 remediation priority, once prioritized.</summary>
    public RiskPriority? Priority { get; private set; }

    /// <summary>UTC instant the risk finding was documented/prioritized, if any.</summary>
    public UtcTimestamp? DocumentedAtUtc { get; private set; }

    // ---- Mitigation (Phase 2) -------------------------------------------------

    private readonly List<ControlMeasure> _controlMeasures = [];

    /// <summary>Control measures applied against this risk, in Hierarchy-of-Controls order.</summary>
    public IReadOnlyList<ControlMeasure> ControlMeasures => _controlMeasures.AsReadOnly();

    /// <summary>Residual risk score recorded after the most recent effectiveness validation, if any.</summary>
    public RiskScore? ResidualScore { get; private set; }

    /// <summary>UTC instant residual risk was last validated, if any.</summary>
    public UtcTimestamp? LastValidatedAtUtc { get; private set; }

    // ---- Reporting & review (Phase 3) --------------------------------------------

    /// <summary>Mandated cadence at which this risk must be formally re-reviewed once mitigated.</summary>
    public RiskReviewCadence ReviewCadence { get; private set; } = RiskReviewCadence.Monthly;

    /// <summary>UTC instant of the most recently completed periodic review, if any.</summary>
    public UtcTimestamp? LastReviewedAtUtc { get; private set; }

    /// <summary>UTC instant the next periodic review is due, if any.</summary>
    public DateTime? NextReviewDueUtc { get; private set; }

    /// <summary>Indicates whether formal review/acknowledgment is required for this record.</summary>
    public bool ReviewRequired { get; private set; } = true;

    /// <summary>Count of personnel/stakeholders who have reviewed or acknowledged this record.</summary>
    public int ReviewCount { get; private set; }

    // ---- Audit ---------------------------------------------------------------

    /// <summary>Monotonically increasing risk revision number, starting at 1. Bumped on every completed review.</summary>
    public int RiskVersion { get; private set; } = 1;

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>UTC instant this record was archived, if any.</summary>
    public UtcTimestamp? ArchivedAtUtc { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IRiskDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IRiskDomainEvent> DomainEvents
        => new ReadOnlyCollection<IRiskDomainEvent>(_domainEvents);

    // For rehydration only.
    private Risk() { }

    // ---- Factory: Identify hazard --------------------------------------------

    /// <summary>Identifies a new hazard and raises <see cref="RiskCreatedEvent"/> (Phase 1, Step 1).</summary>
    public static Risk Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string riskTitle = "",
        string location = "",
        string hazardSource = "",
        UserId ownerId = default,
        bool reviewRequired = true)
    {
        var agg = new Risk
        {
            RiskId        = RiskId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = RiskStatus.Identified,
            RiskTitle     = (riskTitle ?? string.Empty).Trim(),
            Location      = (location ?? string.Empty).Trim(),
            HazardSource  = (hazardSource ?? string.Empty).Trim(),
            Category      = RiskLevel.Default,
            OwnerId       = ownerId,
            ReviewRequired = reviewRequired,
            RiskVersion   = 1,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new RiskCreatedEvent(
            EventId:       RiskEventId.New(),
            RiskId:        agg.RiskId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Phase 1: Assessment --------------------------------------------------

    /// <summary>Records the Severity x Likelihood risk score for the hazard (Phase 1, Step 2).</summary>
    public void ScoreRisk(int severity, int likelihood, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != RiskStatus.Identified && Status != RiskStatus.Scored)
            throw new RiskInvalidStatusTransitionException(RiskId, Status.ToString(), RiskStatus.Scored.ToString(), correlationId, causationId);

        var score = new RiskScore(severity, likelihood);
        CurrentScore = score;
        Category = score.ToRiskLevel();
        Status = RiskStatus.Scored;

        AddDomainEvent(new RiskScoredEvent(
            RiskEventId.New(), RiskId, score, Category, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Prioritizes and documents the risk finding (Phase 1, Steps 3-4).</summary>
    public void Prioritize(RiskPriority? priority, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != RiskStatus.Scored)
            throw new RiskInvalidStatusTransitionException(RiskId, Status.ToString(), RiskStatus.Prioritized.ToString(), correlationId, causationId);

        Priority = priority ?? RiskPriority.FromRiskLevel(Category);
        DocumentedAtUtc = nowUtc;
        Status = RiskStatus.Prioritized;

        AddDomainEvent(new RiskPrioritizedEvent(
            RiskEventId.New(), RiskId, Priority, nowUtc, actor, correlationId, causationId));
    }

    // ---- Phase 2: Mitigation ---------------------------------------------------

    /// <summary>Applies a control measure from the Hierarchy of Controls (Phase 2, Steps 5-6).</summary>
    public ControlMeasure ApplyControlMeasure(
        ControlHierarchyLevel hierarchyLevel, string description, string? justification, UserId responsiblePersonId,
        UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(RiskStatus.Prioritized, RiskStatus.ControlsInProgress);

        if (hierarchyLevel.RequiresJustification && string.IsNullOrWhiteSpace(justification))
            throw new RiskControlJustificationRequiredException(RiskId, hierarchyLevel.Code, correlationId, causationId);

        var measure = new ControlMeasure(RiskId, hierarchyLevel, description, justification, responsiblePersonId, nowUtc);
        _controlMeasures.Add(measure);
        Status = RiskStatus.ControlsInProgress;

        AddDomainEvent(new RiskControlMeasureAppliedEvent(
            RiskEventId.New(), RiskId, measure.ControlMeasureId, hierarchyLevel, nowUtc, actor, correlationId, causationId));

        return measure;
    }

    /// <summary>
    /// Validates residual risk after controls are applied (Phase 2, Step 7). If the residual risk
    /// level is Low or Medium the risk transitions to Mitigated; otherwise it remains
    /// ControlsInProgress so additional controls can be applied, per "If risk remains unacceptable,
    /// additional controls must be applied."
    /// </summary>
    public bool ValidateEffectiveness(int residualSeverity, int residualLikelihood, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != RiskStatus.ControlsInProgress)
            throw new RiskInvalidStatusTransitionException(RiskId, Status.ToString(), RiskStatus.Mitigated.ToString(), correlationId, causationId);

        var residual = new RiskScore(residualSeverity, residualLikelihood);
        ResidualScore = residual;
        LastValidatedAtUtc = nowUtc;

        var residualLevel = residual.ToRiskLevel();
        var isAcceptable = residualLevel.Code is "Low" or "Medium";

        if (isAcceptable)
        {
            Status = RiskStatus.Mitigated;
            LastReviewedAtUtc = nowUtc;
            NextReviewDueUtc = ReviewCadence.ComputeNextDueUtc(nowUtc.Value);
        }

        AddDomainEvent(new RiskEffectivenessValidatedEvent(
            RiskEventId.New(), RiskId, residual, residualLevel, isAcceptable, nowUtc, actor, correlationId, causationId));

        return isAcceptable;
    }

    // ---- Phase 3: Reporting & review -------------------------------------------

    /// <summary>Starts the periodic monitoring/review cycle for a Mitigated risk (Phase 3, Steps 9-10).</summary>
    public void StartReviewCycle(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != RiskStatus.Mitigated)
            throw new RiskInvalidStatusTransitionException(RiskId, Status.ToString(), RiskStatus.UnderReview.ToString(), correlationId, causationId);

        Status = RiskStatus.UnderReview;

        AddDomainEvent(new RiskReviewCycleStartedEvent(
            RiskEventId.New(), RiskId, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Completes a periodic review, bumping the revision and returning the risk to Mitigated.</summary>
    public void CompleteReview(RiskReviewCadence? newCadence, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != RiskStatus.UnderReview)
            throw new RiskInvalidStatusTransitionException(RiskId, Status.ToString(), RiskStatus.Mitigated.ToString(), correlationId, causationId);

        RiskVersion++;
        ReviewCadence = newCadence ?? ReviewCadence;
        Status = RiskStatus.Mitigated;
        LastReviewedAtUtc = nowUtc;
        NextReviewDueUtc = ReviewCadence.ComputeNextDueUtc(nowUtc.Value);
        ReviewCount++;

        AddDomainEvent(new RiskReviewCompletedEvent(
            RiskEventId.New(), RiskId, NextReviewDueUtc.Value, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Formally closes a Mitigated risk.</summary>
    public void Close(UserId closedBy, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(RiskStatus.Mitigated, RiskStatus.UnderReview);

        Status = RiskStatus.Closed;

        AddDomainEvent(new RiskClosedEvent(
            RiskEventId.New(), RiskId, closedBy, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Archives a Closed risk for long-term retention.</summary>
    public void Archive(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != RiskStatus.Closed)
            throw new RiskInvalidStatusTransitionException(RiskId, Status.ToString(), RiskStatus.Archived.ToString(), correlationId, causationId);

        Status = RiskStatus.Archived;
        ArchivedAtUtc = nowUtc;

        AddDomainEvent(new RiskArchivedEvent(
            RiskEventId.New(), RiskId, nowUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IRiskDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params RiskStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Risk {RiskId}.");
    }
}
