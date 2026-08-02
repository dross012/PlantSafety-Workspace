// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Domain
//  File:           HumanFactors.cs
//  Created:        2026-06-17
//  Updated:        2026-08-02 — Phase 4 full build-out: extended from identity-only scaffold
//                  with TenantId/SiteId (standard tenant-scoping fields present on every other
//                  aggregate in this codebase), human-factors/behavioral-risk business fields,
//                  and the full Created->Active->Completed->Closed->Archived lifecycle. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing's identical
//                  build-out shape. Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the HumanFactors bounded context. Tracks a single
//                  human-factors/behavioral-risk assessment record (fatigue, cognitive load,
//                  workload stress, communication). Distinct from Operational.Ergonomics.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.Aggregates;

/// <summary>Aggregate root for the HumanFactors bounded context.</summary>
public sealed class HumanFactors
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Unique identity of this HumanFactors aggregate.</summary>
    public HumanFactorsId HumanFactorsId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public HumanFactorsStatus Status { get; private set; }

    // ---- HumanFactors-specific business properties -----------------------------

    /// <summary>Human-factors classification (e.g. Fatigue, CognitiveLoad, WorkloadStress, Communication).</summary>
    public string FactorType { get; private set; } = string.Empty;

    /// <summary>Identity of the worker affected by this assessment, if applicable.</summary>
    public UserId? AffectedUserId { get; private set; }

    /// <summary>Severity rating on a 1-5 scale.</summary>
    public int SeverityRating { get; private set; }

    /// <summary>Narrative description of the assessed human factor.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IHumanFactorsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IHumanFactorsDomainEvent> DomainEvents
        => new ReadOnlyCollection<IHumanFactorsDomainEvent>(_domainEvents);

    // For rehydration only.
    private HumanFactors() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new HumanFactors aggregate and raises <see cref="HumanFactorsCreatedEvent"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="severityRating"/> is not between 1 and 5.</exception>
    public static HumanFactors Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string factorType = "",
        UserId? affectedUserId = null,
        int severityRating = 1,
        string description = "")
    {
        if (severityRating is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(severityRating), "SeverityRating must be between 1 and 5.");

        var agg = new HumanFactors
        {
            HumanFactorsId  = HumanFactorsId.New(),
            TenantId        = tenantId,
            SiteId          = siteId,
            Status          = HumanFactorsStatus.Created,
            FactorType      = (factorType ?? string.Empty).Trim(),
            AffectedUserId  = affectedUserId,
            SeverityRating  = severityRating,
            Description     = (description ?? string.Empty).Trim(),
            CreatedAtUtc    = createdAtUtc,
            CreatedBy       = createdBy,
        };

        agg._domainEvents.Add(new HumanFactorsCreatedEvent(
            EventId:        Uuid7.New(),
            HumanFactorsId: agg.HumanFactorsId.Value,
            CreatedAtUtc:   createdAtUtc,
            Actor:          actor,
            CorrelationId:  correlationId,
            CausationId:    causationId));

        return agg;
    }

    // ---- Lifecycle: Activate / Complete / Close / Archive -----------------------
    // Phase 4 full build-out (2026-08-02): this aggregate previously had only the Create
    // factory. Mirrors PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing's build-out
    // shape, using the standard Activate/Complete/Close/Archive verbs.

    /// <summary>Activates the human-factors assessment (Created -&gt; Active).</summary>
    public void Activate(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, HumanFactorsLifecycleRules.CanActivate, HumanFactorsStatus.Created);

        Status = HumanFactorsStatus.Active;

        AddDomainEvent(new HumanFactorsActivatedEvent(
            EventId: Uuid7.New(),
            HumanFactorsId: HumanFactorsId.Value,
            ActorUserId: actorUserId,
            ActivatedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Completes the human-factors assessment (Active -&gt; Completed).</summary>
    public void Complete(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, HumanFactorsLifecycleRules.CanComplete, HumanFactorsStatus.Active);

        Status = HumanFactorsStatus.Completed;

        AddDomainEvent(new HumanFactorsCompletedEvent(
            EventId: Uuid7.New(),
            HumanFactorsId: HumanFactorsId.Value,
            ActorUserId: actorUserId,
            CompletedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Closes the human-factors assessment (Completed -&gt; Closed).</summary>
    public void Close(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, HumanFactorsLifecycleRules.CanClose, HumanFactorsStatus.Completed);

        Status = HumanFactorsStatus.Closed;

        AddDomainEvent(new HumanFactorsClosedEvent(
            EventId: Uuid7.New(),
            HumanFactorsId: HumanFactorsId.Value,
            ActorUserId: actorUserId,
            ClosedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Archives the human-factors assessment for long-term retention (Closed -&gt; Archived).</summary>
    public void Archive(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, HumanFactorsLifecycleRules.CanArchive, HumanFactorsStatus.Closed);

        Status = HumanFactorsStatus.Archived;

        AddDomainEvent(new HumanFactorsArchivedEvent(
            EventId: Uuid7.New(),
            HumanFactorsId: HumanFactorsId.Value,
            ActorUserId: actorUserId,
            ArchivedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -------------------------------------------------------------

    private void AddDomainEvent(IHumanFactorsDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<HumanFactorsStatus, bool> guard, HumanFactorsStatus expected)
    {
        if (guard(Status)) return;

        throw new HumanFactorsInvalidStatusTransitionException(
            HumanFactorsId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
