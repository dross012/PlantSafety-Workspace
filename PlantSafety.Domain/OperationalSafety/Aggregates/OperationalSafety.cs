// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Domain
//  File:           OperationalSafety.cs
//  Created:        2026-06-17
//  Updated:        2026-08-02 — Phase 4 full build-out: extended from identity-only scaffold
//                  with TenantId/SiteId (standard tenant-scoping fields present on every other
//                  aggregate in this codebase), operational-safety-oversight business fields,
//                  and the full Created->Active->Completed->Closed->Archived lifecycle. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing's identical
//                  build-out shape. Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the OperationalSafety bounded context. Tracks a single
//                  general operational-safety oversight record: its safety domain, risk level,
//                  accountable owner, and review due date. Distinct from the pre-existing
//                  Operational module and from the pre-existing, unrelated OperationalSafety.
//                  Alerts/Checklists/PPEManagement/RiskManagement/Schedules Infrastructure
//                  interfaces.
// ============================================================================
namespace PlantSafety.Domain.OperationalSafety.Aggregates;

/// <summary>Aggregate root for the OperationalSafety bounded context.</summary>
public sealed class OperationalSafety
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Unique identity of this OperationalSafety aggregate.</summary>
    public OperationalSafetyId OperationalSafetyId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public OperationalSafetyStatus Status { get; private set; }

    // ---- OperationalSafety-specific business properties -------------------------

    /// <summary>Operational-safety domain (e.g. LineOfFire, WorkAtHeight, MobileEquipment).</summary>
    public string SafetyDomain { get; private set; } = string.Empty;

    /// <summary>Risk level classification: Low, Medium, High, Critical.</summary>
    public string RiskLevel { get; private set; } = "Low";

    /// <summary>Identity of the user accountable for this oversight record.</summary>
    public UserId OwnerUserId { get; private set; }

    /// <summary>UTC timestamp by which this record is next due for review, if scheduled.</summary>
    public UtcTimestamp? ReviewDueAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IOperationalSafetyDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IOperationalSafetyDomainEvent> DomainEvents
        => new ReadOnlyCollection<IOperationalSafetyDomainEvent>(_domainEvents);

    // For rehydration only.
    private OperationalSafety() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new OperationalSafety aggregate and raises <see cref="OperationalSafetyCreatedEvent"/>.</summary>
    public static OperationalSafety Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string safetyDomain = "",
        string riskLevel = "Low",
        UserId ownerUserId = default,
        UtcTimestamp? reviewDueAtUtc = null)
    {
        var agg = new OperationalSafety
        {
            OperationalSafetyId = OperationalSafetyId.New(),
            TenantId            = tenantId,
            SiteId              = siteId,
            Status              = OperationalSafetyStatus.Created,
            SafetyDomain        = (safetyDomain ?? string.Empty).Trim(),
            RiskLevel           = (riskLevel ?? "Low").Trim(),
            OwnerUserId         = ownerUserId,
            ReviewDueAtUtc      = reviewDueAtUtc,
            CreatedAtUtc        = createdAtUtc,
            CreatedBy           = createdBy,
        };

        agg._domainEvents.Add(new OperationalSafetyCreatedEvent(
            EventId:              Uuid7.New(),
            OperationalSafetyId:  agg.OperationalSafetyId.Value,
            CreatedAtUtc:         createdAtUtc,
            Actor:                actor,
            CorrelationId:        correlationId,
            CausationId:          causationId));

        return agg;
    }

    // ---- Lifecycle: Activate / Complete / Close / Archive -----------------------
    // Phase 4 full build-out (2026-08-02): this aggregate previously had only the Create
    // factory. Mirrors PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing's build-out
    // shape, using the standard Activate/Complete/Close/Archive verbs.

    /// <summary>Activates the operational-safety oversight record (Created -&gt; Active).</summary>
    public void Activate(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, OperationalSafetyLifecycleRules.CanActivate, OperationalSafetyStatus.Created);

        Status = OperationalSafetyStatus.Active;

        AddDomainEvent(new OperationalSafetyActivatedEvent(
            EventId: Uuid7.New(),
            OperationalSafetyId: OperationalSafetyId.Value,
            ActorUserId: actorUserId,
            ActivatedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Completes the operational-safety oversight record (Active -&gt; Completed).</summary>
    public void Complete(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, OperationalSafetyLifecycleRules.CanComplete, OperationalSafetyStatus.Active);

        Status = OperationalSafetyStatus.Completed;

        AddDomainEvent(new OperationalSafetyCompletedEvent(
            EventId: Uuid7.New(),
            OperationalSafetyId: OperationalSafetyId.Value,
            ActorUserId: actorUserId,
            CompletedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Closes the operational-safety oversight record (Completed -&gt; Closed).</summary>
    public void Close(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, OperationalSafetyLifecycleRules.CanClose, OperationalSafetyStatus.Completed);

        Status = OperationalSafetyStatus.Closed;

        AddDomainEvent(new OperationalSafetyClosedEvent(
            EventId: Uuid7.New(),
            OperationalSafetyId: OperationalSafetyId.Value,
            ActorUserId: actorUserId,
            ClosedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Archives the operational-safety oversight record for long-term retention (Closed -&gt; Archived).</summary>
    public void Archive(
        UserId actorUserId,
        UtcTimestamp atUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, OperationalSafetyLifecycleRules.CanArchive, OperationalSafetyStatus.Closed);

        Status = OperationalSafetyStatus.Archived;

        AddDomainEvent(new OperationalSafetyArchivedEvent(
            EventId: Uuid7.New(),
            OperationalSafetyId: OperationalSafetyId.Value,
            ActorUserId: actorUserId,
            ArchivedAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -------------------------------------------------------------

    private void AddDomainEvent(IOperationalSafetyDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<OperationalSafetyStatus, bool> guard, OperationalSafetyStatus expected)
    {
        if (guard(Status)) return;

        throw new OperationalSafetyInvalidStatusTransitionException(
            OperationalSafetyId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
