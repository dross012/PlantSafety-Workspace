// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Domain
//  File:           Governance.cs
//  Created:        2026-06-17
//  Updated:        2026-08-02 — Phase 4 full build-out: extended from identity-only scaffold
//                  with TenantId/SiteId (standard tenant-scoping fields present on every other
//                  aggregate in this codebase), governance business fields, and the full
//                  Created->Active->Completed->Closed->Archived lifecycle. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing's identical
//                  build-out shape. Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Governance bounded context. Tracks a single
//                  safety-governance/policy-oversight record: its governance area, linked
//                  policy reference, review cadence, and accountable owner.
// ============================================================================
namespace PlantSafety.Domain.Governance.Aggregates;

/// <summary>Aggregate root for the Governance bounded context.</summary>
public sealed class Governance
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Unique identity of this Governance aggregate.</summary>
    public GovernanceId GovernanceId { get; private set; }

    /// <summary>Tenant that owns this governance record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public GovernanceStatus Status { get; private set; }

    // ---- Governance-specific business properties ------------------------------

    /// <summary>Governance area (e.g. PolicyOversight, RiskGovernance, RegulatoryLiaison).</summary>
    public string GovernanceArea { get; private set; } = string.Empty;

    /// <summary>Optional identifier of a linked policy record.</summary>
    public Guid? PolicyReferenceId { get; private set; }

    /// <summary>Cadence, in months, at which this governance record must be reviewed. Defaults to 12.</summary>
    public int ReviewCycleMonths { get; private set; } = 12;

    /// <summary>Identity of the user accountable for this governance record.</summary>
    public UserId OwnerUserId { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IGovernanceDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IGovernanceDomainEvent> DomainEvents
        => new ReadOnlyCollection<IGovernanceDomainEvent>(_domainEvents);

    // For rehydration only.
    private Governance() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Governance aggregate and raises <see cref="GovernanceCreatedEvent"/>.</summary>
    public static Governance Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string governanceArea = "",
        Guid? policyReferenceId = null,
        int reviewCycleMonths = 12,
        UserId ownerUserId = default)
    {
        var agg = new Governance
        {
            GovernanceId      = GovernanceId.New(),
            TenantId          = tenantId,
            SiteId            = siteId,
            Status            = GovernanceStatus.Created,
            GovernanceArea    = (governanceArea ?? string.Empty).Trim(),
            PolicyReferenceId = policyReferenceId,
            ReviewCycleMonths = reviewCycleMonths <= 0 ? 12 : reviewCycleMonths,
            OwnerUserId       = ownerUserId,
            CreatedAtUtc      = createdAtUtc,
            CreatedBy         = createdBy,
        };

        agg._domainEvents.Add(new GovernanceCreatedEvent(
            EventId:       Uuid7.New(),
            GovernanceId:  agg.GovernanceId.Value,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: Activate / Complete / Close / Archive -----------------------
    // Phase 4 full build-out (2026-08-02): this aggregate previously had only the Create
    // factory. Mirrors PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing's build-out
    // shape, using the standard Activate/Complete/Close/Archive verbs.

    /// <summary>Activates the governance record (Created -&gt; Active).</summary>
    public void Activate(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, GovernanceLifecycleRules.CanActivate, GovernanceStatus.Created);

        Status = GovernanceStatus.Active;

        AddDomainEvent(new GovernanceActivatedEvent(
            EventId: Uuid7.New(),
            GovernanceId: GovernanceId.Value,
            ActorUserId: actorUserId,
            ActivatedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Completes the governance record (Active -&gt; Completed).</summary>
    public void Complete(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, GovernanceLifecycleRules.CanComplete, GovernanceStatus.Active);

        Status = GovernanceStatus.Completed;

        AddDomainEvent(new GovernanceCompletedEvent(
            EventId: Uuid7.New(),
            GovernanceId: GovernanceId.Value,
            ActorUserId: actorUserId,
            CompletedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Closes the governance record (Completed -&gt; Closed).</summary>
    public void Close(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, GovernanceLifecycleRules.CanClose, GovernanceStatus.Completed);

        Status = GovernanceStatus.Closed;

        AddDomainEvent(new GovernanceClosedEvent(
            EventId: Uuid7.New(),
            GovernanceId: GovernanceId.Value,
            ActorUserId: actorUserId,
            ClosedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Archives the governance record for long-term retention (Closed -&gt; Archived).</summary>
    public void Archive(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, GovernanceLifecycleRules.CanArchive, GovernanceStatus.Closed);

        Status = GovernanceStatus.Archived;

        AddDomainEvent(new GovernanceArchivedEvent(
            EventId: Uuid7.New(),
            GovernanceId: GovernanceId.Value,
            ActorUserId: actorUserId,
            ArchivedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -------------------------------------------------------------

    private void AddDomainEvent(IGovernanceDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<GovernanceStatus, bool> guard, GovernanceStatus expected)
    {
        if (guard(Status)) return;

        throw new GovernanceInvalidStatusTransitionException(
            GovernanceId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
