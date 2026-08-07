// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Domain
//  File:           IncidentReport.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the PHMSA Incidents bounded context. Models a DOT Form
//                  5800.1 Hazardous Materials Incident Report: incident classification, the
//                  material involved (if known), casualty/damage figures, and the reporting
//                  workflow lifecycle (Draft -> Submitted -> UnderReview -> Closed).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Events;
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Exceptions;
using PlantSafety.Domain.Compliance.PHMSA.Incidents.Rules;
using PlantSafety.Domain.Compliance.PHMSA.Incidents.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Incidents.Aggregates;

/// <summary>Aggregate root for the PHMSA Incidents bounded context (DOT Form 5800.1).</summary>
public sealed partial class IncidentReport
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this IncidentReport aggregate.</summary>
    public IncidentReportId Id { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public IncidentReportStatus Status { get; private set; }

    // ---- Incident classification ----------------------------------------------

    /// <summary>Classification of the incident (Spill, Leak, FireOrExplosion, etc.).</summary>
    public IncidentType IncidentType { get; private set; }

    /// <summary>
    /// Optional reference to the classified hazardous material involved (Classification
    /// module). Null when a report is filed before formal classification lookup, or for a
    /// near-miss with no confirmed material.
    /// </summary>
    public HazmatMaterialId? MaterialInvolvedId { get; private set; }

    /// <summary>Free-text fallback description used when <see cref="MaterialInvolvedId"/> is null.</summary>
    public string? MaterialInvolvedDescription { get; private set; }

    // ---- Casualty / damage figures ---------------------------------------------

    /// <summary>Count of injuries associated with the incident. Always &gt;= 0.</summary>
    public int Injuries { get; private set; }

    /// <summary>Whether the incident involved one or more fatalities.</summary>
    public bool FatalitiesInvolved { get; private set; }

    /// <summary>Estimated property damage in USD, if known. Always &gt;= 0 when provided.</summary>
    public decimal? PropertyDamageEstimateUsd { get; private set; }

    // ---- Narrative and context ---------------------------------------------------

    /// <summary>Required free-text description of the incident.</summary>
    public string Narrative { get; private set; } = string.Empty;

    /// <summary>UTC instant the incident occurred (distinct from <see cref="CreatedAtUtc"/>, when the report was filed).</summary>
    public DateTime IncidentDateUtc { get; private set; }

    /// <summary>Physical location the incident occurred at.</summary>
    public string Location { get; private set; } = string.Empty;

    /// <summary>
    /// Computed flag indicating whether this incident meets the PHMSA reporting threshold.
    /// See <see cref="Rules.IncidentReportabilityRules"/> for the (illustrative) computation.
    /// </summary>
    public bool IsReportableToPhmsa { get; private set; }

    // ---- Authorship ------------------------------------------------------------

    /// <summary>Identity of the user who created this report (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>UTC timestamp the report was filed.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    // ---- Lifecycle attributes ----------------------------------------------------

    /// <summary>UTC instant the report was submitted for review.</summary>
    public UtcTimestamp? SubmittedAtUtc { get; private set; }

    /// <summary>UTC instant review of the report began.</summary>
    public UtcTimestamp? ReviewStartedAtUtc { get; private set; }

    /// <summary>Optional resolution notes recorded when the report was closed.</summary>
    public string? ResolutionNotes { get; private set; }

    /// <summary>UTC instant the report was closed.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IIncidentReportDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IIncidentReportDomainEvent> DomainEvents
        => new ReadOnlyCollection<IIncidentReportDomainEvent>(_domainEvents);

    // For rehydration only.
    private IncidentReport() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>
    /// Creates a new IncidentReport aggregate, computes <see cref="IsReportableToPhmsa"/> via
    /// <see cref="Rules.IncidentReportabilityRules"/>, and raises <see cref="IncidentReportedEvent"/>.
    /// </summary>
    public static IncidentReport Create(
        IncidentType incidentType,
        HazmatMaterialId? materialInvolvedId,
        string? materialInvolvedDescription,
        int injuries,
        bool fatalitiesInvolved,
        decimal? propertyDamageEstimateUsd,
        string narrative,
        DateTime incidentDateUtc,
        string location,
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        if (string.IsNullOrWhiteSpace(narrative))
            throw new InvalidIncidentReportDataException("narrative must not be empty.", correlationId, causationId);

        if (injuries < 0)
            throw new InvalidIncidentReportDataException("injuries must not be negative.", correlationId, causationId);

        if (propertyDamageEstimateUsd is decimal damage && damage < 0m)
            throw new InvalidIncidentReportDataException(
                "propertyDamageEstimateUsd must not be negative when provided.", correlationId, causationId);

        var isReportable = IncidentReportabilityRules.IsReportableToPhmsa(
            incidentType, injuries, fatalitiesInvolved, propertyDamageEstimateUsd);

        var agg = new IncidentReport
        {
            Id                           = IncidentReportId.New(),
            Status                       = IncidentReportStatus.Draft,
            IncidentType                 = incidentType,
            MaterialInvolvedId           = materialInvolvedId,
            MaterialInvolvedDescription  = string.IsNullOrWhiteSpace(materialInvolvedDescription)
                                                ? null
                                                : materialInvolvedDescription.Trim(),
            Injuries                     = injuries,
            FatalitiesInvolved           = fatalitiesInvolved,
            PropertyDamageEstimateUsd    = propertyDamageEstimateUsd,
            Narrative                    = narrative.Trim(),
            IncidentDateUtc              = incidentDateUtc,
            Location                     = (location ?? string.Empty).Trim(),
            IsReportableToPhmsa          = isReportable,
            CreatedBy                    = createdBy,
            CreatedAtUtc                 = createdAtUtc,
        };

        agg.AddDomainEvent(new IncidentReportedEvent(
            EventId:              new IncidentReportEventId(Uuid7.New()),
            IncidentReportId:     agg.Id,
            IncidentType:         agg.IncidentType,
            IsReportableToPhmsa:  agg.IsReportableToPhmsa,
            CreatedAtUtc:         createdAtUtc,
            Actor:                actor,
            CorrelationId:        correlationId,
            CausationId:          causationId));

        return agg;
    }

    // ---- Lifecycle: Submit / BeginReview / Close --------------------------------

    /// <summary>
    /// Submits the report for review (Draft -&gt; Submitted). Valid only from
    /// <see cref="IncidentReportStatus.Draft"/>.
    /// </summary>
    public void Submit(
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        EnsureStatusIsOneOf(correlationId, causationId, IncidentReportLifecycleRules.CanSubmit, IncidentReportStatus.Draft);

        var submittedAtUtc = UtcTimestamp.Now();
        SubmittedAtUtc = submittedAtUtc;
        Status = IncidentReportStatus.Submitted;

        AddDomainEvent(new IncidentReportSubmittedEvent(
            EventId: new IncidentReportEventId(Uuid7.New()),
            IncidentReportId: Id,
            SubmittedAtUtc: submittedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Begins formal review of a submitted report (Submitted -&gt; UnderReview). Valid only from
    /// <see cref="IncidentReportStatus.Submitted"/>.
    /// </summary>
    public void BeginReview(
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        EnsureStatusIsOneOf(correlationId, causationId, IncidentReportLifecycleRules.CanBeginReview, IncidentReportStatus.Submitted);

        var reviewStartedAtUtc = UtcTimestamp.Now();
        ReviewStartedAtUtc = reviewStartedAtUtc;
        Status = IncidentReportStatus.UnderReview;

        AddDomainEvent(new IncidentReportReviewStartedEvent(
            EventId: new IncidentReportEventId(Uuid7.New()),
            IncidentReportId: Id,
            ReviewStartedAtUtc: reviewStartedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Formally closes a reviewed report (UnderReview -&gt; Closed). Valid only from
    /// <see cref="IncidentReportStatus.UnderReview"/>.
    /// </summary>
    public void Close(
        string? resolutionNotes,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        EnsureStatusIsOneOf(correlationId, causationId, IncidentReportLifecycleRules.CanClose, IncidentReportStatus.UnderReview);

        var closedAtUtc = UtcTimestamp.Now();
        ResolutionNotes = string.IsNullOrWhiteSpace(resolutionNotes) ? null : resolutionNotes.Trim();
        ClosedAtUtc = closedAtUtc;
        Status = IncidentReportStatus.Closed;

        AddDomainEvent(new IncidentReportClosedEvent(
            EventId: new IncidentReportEventId(Uuid7.New()),
            IncidentReportId: Id,
            ResolutionNotes: ResolutionNotes,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IIncidentReportDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<IncidentReportStatus, bool> guard, IncidentReportStatus expected)
    {
        if (guard(Status)) return;

        throw new IncidentReportInvalidStatusTransitionException(
            Id, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
