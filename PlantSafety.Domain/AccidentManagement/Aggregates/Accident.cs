// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           Accident.cs
//  Created:        2026-07-20
//  Updated:        2026-07-21 -- replaced the generic single-Create() placeholder with the real
//                  incident-management lifecycle documented in Accident Management: "Securing the
//                  site, triaging injuries, and executing emergency plans. Investigation & Root
//                  Cause Analysis... Corrective Actions (CAPA)... Regulatory Compliance:
//                  Documenting events and maintaining audit trails for authorities." Corrective
//                  actions are modeled as a related child entity (see Entities/CorrectiveAction.cs)
//                  because the source document treats CAPA as a distinct tracked concept with its
//                  own responsible person, deadline, verification, and evidence.
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the AccidentManagement bounded context. Tracks a single
//                  workplace accident/incident from report through investigation, root-cause
//                  analysis, corrective action, regulatory filing, and closure.
//                  Additive-only evolution.
// ============================================================================
using System.Collections.ObjectModel;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Common.Abstractions;
using PlantSafety.Domain.AccidentManagement.Entities;
using PlantSafety.Domain.AccidentManagement.Enums;
using PlantSafety.Domain.AccidentManagement.Events;
using PlantSafety.Domain.AccidentManagement.Exceptions;
using PlantSafety.Domain.AccidentManagement.ValueObjects.Ids;

namespace PlantSafety.Domain.AccidentManagement.Aggregates;

/// <summary>Aggregate root for the AccidentManagement bounded context.</summary>
public sealed partial class Accident
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Accident aggregate.</summary>
    public AccidentId AccidentId { get; private set; }

    /// <summary>Tenant that owns this accident record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this accident is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current investigation lifecycle status.</summary>
    public AccidentStatus Status { get; private set; }

    // ---- Report details ------------------------------------------------------

    /// <summary>Human-readable summary of the accident (e.g. "Forklift collision, Dock 3").</summary>
    public string AccidentTitle { get; private set; } = string.Empty;

    /// <summary>Free-form location description (building/zone/machine).</summary>
    public string Location { get; private set; } = string.Empty;

    /// <summary>OSHA-aligned injury severity classification.</summary>
    public AccidentSeverity Severity { get; private set; } = AccidentSeverity.Default;

    /// <summary>Count of individuals injured in this accident.</summary>
    public int InjuredPersonCount { get; private set; }

    /// <summary>UTC instant the accident actually occurred (may differ from report time).</summary>
    public UtcTimestamp OccurredAtUtc { get; private set; }

    /// <summary>Current phase-of-investigation classifier (site securing/triage, RCA, CAPA, regulatory).</summary>
    public InvestigationPhase Category { get; private set; } = InvestigationPhase.Default;

    /// <summary>Whether site securing and injury triage has been confirmed complete.</summary>
    public bool SiteSecuredAndTriaged { get; private set; }

    // ---- Investigation ---------------------------------------------------------

    /// <summary>Identity of the assigned investigator, once assigned.</summary>
    public UserId? InvestigatorId { get; private set; }

    /// <summary>UTC instant an investigator was assigned, if any.</summary>
    public UtcTimestamp? InvestigatorAssignedAtUtc { get; private set; }

    /// <summary>Root cause hazard category identified by RCA, once recorded.</summary>
    public RootCauseCategory? RootCause { get; private set; }

    /// <summary>Free-form RCA narrative/findings.</summary>
    public string? RootCauseNotes { get; private set; }

    /// <summary>UTC instant the root cause was recorded, if any.</summary>
    public UtcTimestamp? RootCauseRecordedAtUtc { get; private set; }

    // ---- Corrective actions (CAPA) -----------------------------------------------

    private readonly List<CorrectiveAction> _correctiveActions = [];

    /// <summary>CAPA corrective actions tracked against this investigation.</summary>
    public IReadOnlyList<CorrectiveAction> CorrectiveActions => _correctiveActions.AsReadOnly();

    // ---- Regulatory compliance --------------------------------------------------

    /// <summary>Whether this accident's severity mandates formal regulatory filing (e.g. OSHA 300/301).</summary>
    public bool RegulatoryFilingRequired { get; private set; }

    /// <summary>Regulatory agency the filing was made to, once filed.</summary>
    public string? RegulatoryAgency { get; private set; }

    /// <summary>Regulatory filing reference/confirmation number, once filed.</summary>
    public string? RegulatoryReferenceNumber { get; private set; }

    /// <summary>UTC instant the regulatory filing was completed, if any.</summary>
    public UtcTimestamp? RegulatoryFiledAtUtc { get; private set; }

    // ---- Closure -----------------------------------------------------------

    /// <summary>UTC instant the investigation was closed, if any.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    /// <summary>UTC instant this record was archived, if any.</summary>
    public UtcTimestamp? ArchivedAtUtc { get; private set; }

    // ---- Audit ---------------------------------------------------------------

    /// <summary>Identity of the accountable owner (typically the reporting supervisor or safety manager).</summary>
    public UserId OwnerId { get; private set; }

    /// <summary>Indicates whether formal review/acknowledgment is required for this record.</summary>
    public bool ReviewRequired { get; private set; } = true;

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IAccidentDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAccidentDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAccidentDomainEvent>(_domainEvents);

    // For rehydration only.
    private Accident() { }

    // ---- Factory: ReportAccident ---------------------------------------------

    /// <summary>Reports a new Accident and raises <see cref="AccidentCreatedEvent"/>.</summary>
    public static Accident Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string accidentTitle = "",
        string location = "",
        AccidentSeverity? severity = null,
        int injuredPersonCount = 0,
        UtcTimestamp? occurredAtUtc = null,
        InvestigationPhase? category = null,
        UserId ownerId = default,
        bool reviewRequired = true)
    {
        var effectiveSeverity = severity ?? AccidentSeverity.Default;
        var agg = new Accident
        {
            AccidentId               = AccidentId.New(),
            TenantId                 = tenantId,
            SiteId                   = siteId,
            Status                   = AccidentStatus.Reported,
            AccidentTitle            = (accidentTitle ?? string.Empty).Trim(),
            Location                 = (location ?? string.Empty).Trim(),
            Severity                 = effectiveSeverity,
            InjuredPersonCount       = injuredPersonCount < 0 ? 0 : injuredPersonCount,
            OccurredAtUtc            = occurredAtUtc ?? createdAtUtc,
            Category                 = category ?? InvestigationPhase.Default,
            RegulatoryFilingRequired = effectiveSeverity.IsOshaRecordable,
            OwnerId                  = ownerId,
            ReviewRequired           = reviewRequired,
            CreatedAtUtc             = createdAtUtc,
            CreatedBy                = createdBy,
        };

        agg.AddDomainEvent(new AccidentCreatedEvent(
            EventId:       AccidentEventId.New(),
            AccidentId:    agg.AccidentId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: Site securing / triage -----------------------------------

    /// <summary>Confirms the site has been secured and injured personnel triaged.</summary>
    public void ConfirmSiteSecuredAndTriaged()
    {
        EnsureStatusIsOneOf(AccidentStatus.Reported);
        SiteSecuredAndTriaged = true;
        Category = InvestigationPhase.InvestigationRootCauseAnalysis;
    }

    // ---- Lifecycle: Investigation --------------------------------------------

    /// <summary>Assigns an investigator and moves the accident into active investigation.</summary>
    public void AssignInvestigator(UserId investigatorId, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != AccidentStatus.Reported)
            throw new AccidentInvalidStatusTransitionException(AccidentId, Status.ToString(), AccidentStatus.UnderInvestigation.ToString(), correlationId, causationId);

        Status = AccidentStatus.UnderInvestigation;
        InvestigatorId = investigatorId;
        InvestigatorAssignedAtUtc = nowUtc;
        Category = InvestigationPhase.InvestigationRootCauseAnalysis;

        AddDomainEvent(new AccidentInvestigatorAssignedEvent(
            AccidentEventId.New(), AccidentId, investigatorId, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Records the outcome of Root Cause Analysis, per Full Risk Management Process (RCA/FMEA).</summary>
    public void RecordRootCause(RootCauseCategory rootCause, string notes, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != AccidentStatus.UnderInvestigation)
            throw new AccidentInvalidStatusTransitionException(AccidentId, Status.ToString(), AccidentStatus.RootCauseIdentified.ToString(), correlationId, causationId);

        Status = AccidentStatus.RootCauseIdentified;
        RootCause = rootCause;
        RootCauseNotes = (notes ?? string.Empty).Trim();
        RootCauseRecordedAtUtc = nowUtc;

        AddDomainEvent(new AccidentRootCauseRecordedEvent(
            AccidentEventId.New(), AccidentId, rootCause, nowUtc, actor, correlationId, causationId));
    }

    // ---- Lifecycle: Corrective actions (CAPA) --------------------------------

    /// <summary>Creates a new CAPA corrective action against this investigation (Full Risk Management Process, Step 6).</summary>
    public CorrectiveAction CreateCorrectiveAction(
        CorrectiveActionCategory category, string description, UserId responsiblePersonId, UtcTimestamp dueAtUtc,
        UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(AccidentStatus.RootCauseIdentified, AccidentStatus.CorrectiveActionsInProgress);

        var action = new CorrectiveAction(AccidentId, category, description, responsiblePersonId, dueAtUtc, nowUtc);
        _correctiveActions.Add(action);
        Status = AccidentStatus.CorrectiveActionsInProgress;
        Category = InvestigationPhase.CorrectiveActionsCAPA;

        AddDomainEvent(new AccidentCorrectiveActionCreatedEvent(
            AccidentEventId.New(), AccidentId, action.CorrectiveActionId, category, responsiblePersonId, dueAtUtc, nowUtc, actor, correlationId, causationId));

        return action;
    }

    /// <summary>Marks a tracked corrective action Completed with evidence.</summary>
    public void CompleteCorrectiveAction(CorrectiveActionId correctiveActionId, string completionEvidence, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        var action = _correctiveActions.FirstOrDefault(a => a.CorrectiveActionId == correctiveActionId)
            ?? throw new CorrectiveActionNotFoundException(AccidentId, correctiveActionId, correlationId, causationId);

        action.Complete(completionEvidence, nowUtc);

        AddDomainEvent(new AccidentCorrectiveActionCompletedEvent(
            AccidentEventId.New(), AccidentId, correctiveActionId, nowUtc, actor, correlationId, causationId));
    }

    // ---- Lifecycle: Regulatory & closure --------------------------------------

    /// <summary>Records completion of required regulatory documentation (OSHA 300/300A/301, etc.).</summary>
    public void FileRegulatoryReport(UserId filedBy, string regulatoryAgency, string referenceNumber, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(AccidentStatus.CorrectiveActionsInProgress, AccidentStatus.RootCauseIdentified);

        Status = AccidentStatus.RegulatoryFilingComplete;
        RegulatoryAgency = (regulatoryAgency ?? string.Empty).Trim();
        RegulatoryReferenceNumber = (referenceNumber ?? string.Empty).Trim();
        RegulatoryFiledAtUtc = nowUtc;
        Category = InvestigationPhase.RegulatoryComplianceDocumentation;

        AddDomainEvent(new AccidentRegulatoryFilingCompletedEvent(
            AccidentEventId.New(), AccidentId, filedBy, RegulatoryAgency, RegulatoryReferenceNumber, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Closes the investigation once all mandatory prerequisites (RCA, CAPA, regulatory filing if required) are satisfied.</summary>
    public void CloseInvestigation(UserId closedBy, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(AccidentStatus.CorrectiveActionsInProgress, AccidentStatus.RegulatoryFilingComplete);

        if (RegulatoryFilingRequired && Status != AccidentStatus.RegulatoryFilingComplete)
            throw new AccidentRegulatoryFilingRequiredException(AccidentId, correlationId, causationId);

        Status = AccidentStatus.Closed;
        ClosedAtUtc = nowUtc;

        AddDomainEvent(new AccidentInvestigationClosedEvent(
            AccidentEventId.New(), AccidentId, closedBy, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Archives a Closed accident record for long-term retention.</summary>
    public void Archive(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != AccidentStatus.Closed)
            throw new AccidentInvalidStatusTransitionException(AccidentId, Status.ToString(), AccidentStatus.Archived.ToString(), correlationId, causationId);

        Status = AccidentStatus.Archived;
        ArchivedAtUtc = nowUtc;

        AddDomainEvent(new AccidentArchivedEvent(
            AccidentEventId.New(), AccidentId, nowUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IAccidentDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params AccidentStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Accident {AccidentId}.");
    }
}
