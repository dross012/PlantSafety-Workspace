// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Domain
//  File:           Observation.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  field-observation record shape grounded in
//                  project_plantsafety_domains.md Module 11 (Observation: "Field-level
//                  observations, hazard detection, behavior classification").
//                  Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Observation bounded context. Tracks a
//                  single field observation: its classification (safe/at-risk),
//                  hazard category, location, description, and follow-up state.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.Observation.Aggregates;

/// <summary>Aggregate root for the Observation bounded context.</summary>
public sealed partial class Observation
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Observation aggregate.</summary>
    public ObservationId ObservationId { get; private set; }

    /// <summary>Tenant that owns this observation record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this observation is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ObservationStatus Status { get; private set; }

    // ---- Observation-specific business properties --------------------------------

    /// <summary>Identity of the worker/observer who captured this observation.</summary>
    public UserId ObserverId { get; private set; }

    /// <summary>Observation classification: SafeBehavior, AtRiskBehavior, HazardCondition.</summary>
    public string ObservationType { get; private set; } = "SafeBehavior";

    /// <summary>Hazard category, when the observation identifies a hazard (e.g., Slip/Trip, Ergonomic, PPE).</summary>
    public string HazardCategory { get; private set; } = string.Empty;

    /// <summary>Physical location the observation was made at.</summary>
    public string Location { get; private set; } = string.Empty;

    /// <summary>Narrative description of what was observed.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Indicates whether corrective action is required as a result of this observation.</summary>
    public bool CorrectiveActionRequired { get; private set; }

    /// <summary>UTC timestamp any required follow-up is due.</summary>
    public UtcTimestamp? FollowUpDueAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IObservationDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IObservationDomainEvent> DomainEvents
        => new ReadOnlyCollection<IObservationDomainEvent>(_domainEvents);

    // For rehydration only.
    private Observation() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Observation aggregate and raises <see cref="ObservationCreatedEvent"/>.</summary>
    public static Observation Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        UserId observerId = default,
        string observationType = "SafeBehavior",
        string hazardCategory = "",
        string location = "",
        string description = "",
        bool correctiveActionRequired = false,
        UtcTimestamp? followUpDueAtUtc = null)
    {
        var agg = new Observation
        {
            ObservationId               = ObservationId.New(),
            TenantId                    = tenantId,
            SiteId                      = siteId,
            Status                      = ObservationStatus.Created,
            ObserverId                  = observerId,
            ObservationType             = (observationType ?? "SafeBehavior").Trim(),
            HazardCategory              = (hazardCategory ?? string.Empty).Trim(),
            Location                    = (location ?? string.Empty).Trim(),
            Description                 = (description ?? string.Empty).Trim(),
            CorrectiveActionRequired    = correctiveActionRequired,
            FollowUpDueAtUtc            = followUpDueAtUtc,
            CreatedAtUtc                = createdAtUtc,
            CreatedBy                   = createdBy,
        };

        agg.AddDomainEvent(new ObservationCreatedEvent(
            EventId:       new ObservationEventId(Uuid7.New()),
            ObservationId:      agg.ObservationId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: Activate / Complete ---------------------------------------

    /// <summary>
    /// Begins follow-up tracking for an observation requiring corrective action (Created -&gt;
    /// Active). Valid only from <see cref="ObservationStatus.Created"/>.
    /// </summary>
    public void Activate(
        UserId activatedBy,
        UtcTimestamp activatedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ObservationLifecycleRules.CanActivate, ObservationStatus.Active);

        Status = ObservationStatus.Active;

        AddDomainEvent(new ObservationActivatedEvent(
            EventId: new ObservationEventId(Uuid7.New()),
            ObservationId: ObservationId,
            ActivatedBy: activatedBy,
            ActivatedAtUtc: activatedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Records completion of the required follow-up/corrective action (Active -&gt; Completed).
    /// Valid only from <see cref="ObservationStatus.Active"/>.
    /// </summary>
    public void Complete(
        UserId completedBy,
        string? resolutionNotes,
        UtcTimestamp completedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ObservationLifecycleRules.CanComplete, ObservationStatus.Completed);

        Status = ObservationStatus.Completed;

        AddDomainEvent(new ObservationCompletedEvent(
            EventId: new ObservationEventId(Uuid7.New()),
            ObservationId: ObservationId,
            CompletedBy: completedBy,
            ResolutionNotes: string.IsNullOrWhiteSpace(resolutionNotes) ? null : resolutionNotes.Trim(),
            CompletedAtUtc: completedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Close / Archive -------------------------------------------

    /// <summary>
    /// Closes a completed observation record (Completed -&gt; Closed). Valid only from
    /// <see cref="ObservationStatus.Completed"/>.
    /// </summary>
    public void Close(
        UserId closedBy,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ObservationLifecycleRules.CanClose, ObservationStatus.Closed);

        Status = ObservationStatus.Closed;

        AddDomainEvent(new ObservationClosedEvent(
            EventId: new ObservationEventId(Uuid7.New()),
            ObservationId: ObservationId,
            ClosedBy: closedBy,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Archives a closed observation record for retention (Closed -&gt; Archived). Valid only from
    /// <see cref="ObservationStatus.Closed"/>.
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ObservationLifecycleRules.CanArchive, ObservationStatus.Archived);

        Status = ObservationStatus.Archived;

        AddDomainEvent(new ObservationArchivedEvent(
            EventId: new ObservationEventId(Uuid7.New()),
            ObservationId: ObservationId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IObservationDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ObservationStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Observation {ObservationId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<ObservationStatus, bool> guard, ObservationStatus expected)
    {
        if (guard(Status)) return;

        throw new ObservationInvalidStatusTransitionException(
            ObservationId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
