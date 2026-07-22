// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.StopWorkEvent
//  Layer:          Domain
//  File:           StopWorkEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root representing a single exercise of employee Stop-Work Authority: the non-punitive record of an employee halting work due to a perceived hazard, its resolution, and (optionally) its recognition.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyCulture.StopWorkEvent.Aggregates;

/// <summary>Aggregate root for the StopWorkEvent bounded context.</summary>
public sealed partial class StopWorkEvent
{
    // ---- Identity ------------------------------------------------------

    /// <summary>Strong-typed identity of this StopWorkEvent aggregate.</summary>
    public StopWorkEventId StopWorkEventId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public StopWorkEventStatus Status { get; private set; }

    // ---- Business properties --------------------------------------------
    /// <summary>Work area/location where the stop-work action was initiated.</summary>
    public string WorkArea { get; private set; } = string.Empty;

    /// <summary>Description of the hazard or unsafe condition that triggered the stop-work action.</summary>
    public string HazardDescription { get; private set; } = string.Empty;

    /// <summary>Domain-action identity of the employee/actor who exercised stop-work authority.</summary>
    public PerformedByActor InitiatedByActor { get; private set; }

    /// <summary>Narrative summary of how the stop-work event was resolved, once known.</summary>
    public string? ResolutionSummary { get; private set; }

    /// <summary>Whether this stop-work action was formally recognized/celebrated (Recognition and Reinforcement program).</summary>
    public bool WasCelebrated { get; private set; }

    /// <summary>
    /// Optional cross-module reference (by StrongId, not a compiled dependency) to the
    /// Compliance.Permits permit active at the time work was stopped. Per PlantSafety Program
    /// Structure "Safety Culture Module" §9: "Permits → behavioral compliance during high-risk work".
    /// </summary>
    public Guid? RelatedPermitId { get; private set; }

    /// <summary>
    /// Optional cross-module reference (by StrongId, not a compiled dependency) to the
    /// Checklists.ChecklistCompletion walk-around/inspection during which this stop-work action
    /// was raised. Per PlantSafety Program Structure "Safety Culture Module" §9:
    /// "Checklists → leadership walk-around documentation".
    /// </summary>
    public Guid? RelatedChecklistCompletionId { get; private set; }

    /// <summary>Findings/narrative recorded while investigating this stop-work event, once investigated.</summary>
    public string? InvestigationFindings { get; private set; }

    /// <summary>UTC instant the investigation was recorded, once investigated.</summary>
    public UtcTimestamp? InvestigatedAtUtc { get; private set; }

    /// <summary>UTC instant this event was resolved, once resolved.</summary>
    public UtcTimestamp? ResolvedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------

    private readonly List<IStopWorkEventDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IStopWorkEventDomainEvent> DomainEvents
        => new ReadOnlyCollection<IStopWorkEventDomainEvent>(_domainEvents);

    // For rehydration only.
    private StopWorkEvent() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new StopWorkEvent aggregate and raises <see cref="StopWorkEventCreatedEvent"/>.</summary>
    /// <param name="createdBy">CRUD authorship identity.</param>
    /// <param name="createdAtUtc">Creation timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="siteId">Optional owning site.</param>
    public static StopWorkEvent Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string workArea = "",
        string hazardDescription = "",
        PerformedByActor initiatedByActor = default,
        bool wasCelebrated = false,
        Guid? relatedPermitId = null,
        Guid? relatedChecklistCompletionId = null)
    {
        var agg = new StopWorkEvent
        {
            StopWorkEventId      = StopWorkEventId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = StopWorkEventStatus.Created,
            WorkArea          = (workArea ?? string.Empty).Trim(),
            HazardDescription = (hazardDescription ?? string.Empty).Trim(),
            InitiatedByActor  = initiatedByActor,
            WasCelebrated     = wasCelebrated,
            RelatedPermitId              = relatedPermitId,
            RelatedChecklistCompletionId = relatedChecklistCompletionId,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new StopWorkEventCreatedEvent(
            EventId:       new StopWorkEventEventId(Uuid7.New()),
            StopWorkEventId:      agg.StopWorkEventId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle behavior --------------------------------------------------

    /// <summary>
    /// Records a Just Culture (non-punitive) investigation of this stop-work action: distinguishes
    /// human error from negligence and captures the root-cause findings. (Created/Active → Active).
    /// </summary>
    /// <exception cref="StopWorkEventInvalidStatusTransitionException">Thrown when the event has already been resolved or archived.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="findings"/> is empty.</exception>
    public void Investigate(
        string findings,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        UtcTimestamp occurredAtUtc)
    {
        EnsureStatusIsOneOf(StopWorkEventStatus.Created, StopWorkEventStatus.Active);

        if (string.IsNullOrWhiteSpace(findings))
            throw new ArgumentException("Investigation findings must not be empty.", nameof(findings));

        InvestigationFindings = findings.Trim();
        InvestigatedAtUtc = occurredAtUtc;
        Status = StopWorkEventStatus.Active;

        AddDomainEvent(new StopWorkEventInvestigatedEvent(
            EventId: new StopWorkEventEventId(Uuid7.New()),
            StopWorkEventId: StopWorkEventId,
            Findings: InvestigationFindings,
            InvestigatedAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Resolves this stop-work event (Active → Closed), recording how it was resolved and
    /// optionally recognizing the initiating employee (Recognition &amp; Reinforcement program).
    /// </summary>
    /// <exception cref="StopWorkEventInvalidStatusTransitionException">Thrown when the event is not open for resolution.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="resolutionSummary"/> is empty.</exception>
    public void Resolve(
        string resolutionSummary,
        bool wasCelebrated,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId,
        UtcTimestamp occurredAtUtc)
    {
        EnsureStatusIsOneOf(StopWorkEventStatus.Created, StopWorkEventStatus.Active);

        if (string.IsNullOrWhiteSpace(resolutionSummary))
            throw new ArgumentException("Resolution summary must not be empty.", nameof(resolutionSummary));

        ResolutionSummary = resolutionSummary.Trim();
        WasCelebrated = wasCelebrated;
        ResolvedAtUtc = occurredAtUtc;
        Status = StopWorkEventStatus.Closed;

        AddDomainEvent(new StopWorkEventResolvedEvent(
            EventId: new StopWorkEventEventId(Uuid7.New()),
            StopWorkEventId: StopWorkEventId,
            ResolutionSummary: ResolutionSummary,
            WasCelebrated: WasCelebrated,
            ResolvedAtUtc: occurredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IStopWorkEventDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params StopWorkEventStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            "Invalid lifecycle transition from '" + Status + "' for StopWorkEvent " + StopWorkEventId + ".");
    }
}