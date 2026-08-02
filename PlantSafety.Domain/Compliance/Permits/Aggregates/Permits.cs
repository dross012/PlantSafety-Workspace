// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Domain
//  File:           Permits.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  Permit-to-Work record shape grounded in project_plantsafety_domains.md
//                  Module 12 (Permits: "Permit-to-Work lifecycle management", flagged
//                  as a Critical Module). Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Permits bounded context. Tracks a single
//                  permit-to-work: its type, work description, location, hazards, and
//                  issue/expiry window.
//                  Additive-only evolution.
//  UPDATED:        2026-07-30 -- Phase 4 full build-out: added Approve/Reject/Complete/Expire/
//                  Close/Archive lifecycle methods. Previously this aggregate had only a Create
//                  factory -- PermitsStatus/PermitsLifecycleRules/PermitsArchivalPolicy were fully
//                  scaffolded but nothing called them, and no command in the Application layer
//                  ever loaded or mutated this aggregate (confirmed by
//                  docs/architecture/scheduling-maintenance-compliance-extraction-blueprint.md
//                  Sec 1.2: only 3 write actions existed across ~124 Compliance API endpoints, none
//                  touching a Domain aggregate). Mirrors PermitToWork.PermitWorkflow's lifecycle
//                  shape and guard/event/exception conventions.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Permits.Aggregates;

/// <summary>Aggregate root for the Permits bounded context.</summary>
public sealed partial class Permits
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Permits aggregate.</summary>
    public PermitsId PermitsId { get; private set; }

    /// <summary>Tenant that owns this permit record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this permit is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PermitsStatus Status { get; private set; }

    // ---- Permit-specific business properties --------------------------------

    /// <summary>Permit classification (e.g., HotWork, ConfinedSpace, Excavation, LOTO).</summary>
    public string PermitType { get; private set; } = string.Empty;

    /// <summary>Human-readable permit number/reference.</summary>
    public string PermitNumber { get; private set; } = string.Empty;

    /// <summary>Description of the work covered by this permit.</summary>
    public string WorkDescription { get; private set; } = string.Empty;

    /// <summary>Physical location the permit applies to.</summary>
    public string Location { get; private set; } = string.Empty;

    /// <summary>Hazards identified for this permitted work.</summary>
    public string HazardsIdentified { get; private set; } = string.Empty;

    /// <summary>Identity of the user who requested the permit.</summary>
    public UserId RequestedByUserId { get; private set; }

    /// <summary>Identity of the user who approved/issued the permit, if issued.</summary>
    public UserId? ApprovedByUserId { get; private set; }

    /// <summary>UTC timestamp the permit was issued.</summary>
    public UtcTimestamp? IssuedAtUtc { get; private set; }

    /// <summary>UTC timestamp the permit expires.</summary>
    public UtcTimestamp? ExpiresAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Lifecycle attributes (Approve/Reject/Complete/Expire/Close/Archive) -

    /// <summary>Reason recorded when a permit request is rejected.</summary>
    public string? RejectedReason { get; private set; }

    /// <summary>UTC instant the permit request was rejected.</summary>
    public UtcTimestamp? RejectedAtUtc { get; private set; }

    /// <summary>UTC instant the permitted work was marked completed.</summary>
    public UtcTimestamp? CompletedAtUtc { get; private set; }

    /// <summary>UTC instant an issued permit expired without being completed/closed.</summary>
    public UtcTimestamp? ExpiredAtUtc { get; private set; }

    /// <summary>UTC instant the permit was closed.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    /// <summary>UTC instant the permit was archived for compliance retention.</summary>
    public UtcTimestamp? ArchivedAtUtc { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPermitsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPermitsDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPermitsDomainEvent>(_domainEvents);

    // For rehydration only.
    private Permits() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Permits aggregate and raises <see cref="PermitsCreatedEvent"/>.</summary>
    public static Permits Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string permitType = "",
        string permitNumber = "",
        string workDescription = "",
        string location = "",
        string hazardsIdentified = "",
        UserId requestedByUserId = default,
        UserId? approvedByUserId = null,
        UtcTimestamp? issuedAtUtc = null,
        UtcTimestamp? expiresAtUtc = null)
    {
        var agg = new Permits
        {
            PermitsId           = PermitsId.New(),
            TenantId             = tenantId,
            SiteId               = siteId,
            Status               = PermitsStatus.Created,
            PermitType           = (permitType ?? string.Empty).Trim(),
            PermitNumber         = (permitNumber ?? string.Empty).Trim(),
            WorkDescription      = (workDescription ?? string.Empty).Trim(),
            Location             = (location ?? string.Empty).Trim(),
            HazardsIdentified    = (hazardsIdentified ?? string.Empty).Trim(),
            RequestedByUserId    = requestedByUserId,
            ApprovedByUserId     = approvedByUserId,
            IssuedAtUtc          = issuedAtUtc,
            ExpiresAtUtc         = expiresAtUtc,
            CreatedAtUtc         = createdAtUtc,
            CreatedBy            = createdBy,
        };

        agg.AddDomainEvent(new PermitsCreatedEvent(
            EventId:       new PermitsEventId(Uuid7.New()),
            PermitsId:      agg.PermitsId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: Approve / Reject -----------------------------------------

    /// <summary>
    /// Approves and issues the permit request (Created -&gt; Active), computing its expiry from
    /// <paramref name="validityHours"/> when supplied. Valid only from <see cref="PermitsStatus.Created"/>.
    /// </summary>
    public void Approve(
        UserId approvedByUserId,
        UtcTimestamp approvedAtUtc,
        int? validityHours,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PermitsLifecycleRules.CanActivate, PermitsStatus.Created);

        ApprovedByUserId = approvedByUserId;
        IssuedAtUtc = approvedAtUtc;
        ExpiresAtUtc = validityHours is > 0
            ? UtcTimestamp.From(approvedAtUtc.Value.AddHours(validityHours.Value))
            : null;
        Status = PermitsStatus.Active;

        AddDomainEvent(new PermitsApprovedEvent(
            EventId: new PermitsEventId(Uuid7.New()),
            PermitsId: PermitsId,
            ApprovedByUserId: approvedByUserId,
            ApprovedAtUtc: approvedAtUtc,
            ExpiresAtUtc: ExpiresAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Declines a permit request before approval (Created -&gt; Rejected). Valid only from
    /// <see cref="PermitsStatus.Created"/>.
    /// </summary>
    public void Reject(
        UserId rejectedByUserId,
        string reason,
        UtcTimestamp rejectedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PermitsLifecycleRules.CanReject, PermitsStatus.Created);
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A rejection reason must be supplied.", nameof(reason));

        RejectedReason = reason.Trim();
        RejectedAtUtc = rejectedAtUtc;
        Status = PermitsStatus.Rejected;

        AddDomainEvent(new PermitsRejectedEvent(
            EventId: new PermitsEventId(Uuid7.New()),
            PermitsId: PermitsId,
            RejectedByUserId: rejectedByUserId,
            Reason: RejectedReason,
            RejectedAtUtc: rejectedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Complete / Expire ----------------------------------------

    /// <summary>
    /// Marks the permitted work completed (Active -&gt; Completed). Valid only from
    /// <see cref="PermitsStatus.Active"/>.
    /// </summary>
    public void Complete(
        UtcTimestamp completedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PermitsLifecycleRules.CanComplete, PermitsStatus.Active);

        CompletedAtUtc = completedAtUtc;
        Status = PermitsStatus.Completed;

        AddDomainEvent(new PermitsCompletedEvent(
            EventId: new PermitsEventId(Uuid7.New()),
            PermitsId: PermitsId,
            CompletedAtUtc: completedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Expires an issued permit that passed its validity window without being completed or closed
    /// (Active -&gt; Expired). Valid only from <see cref="PermitsStatus.Active"/>.
    /// </summary>
    public void Expire(
        UtcTimestamp expiredAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PermitsLifecycleRules.CanExpire, PermitsStatus.Active);

        ExpiredAtUtc = expiredAtUtc;
        Status = PermitsStatus.Expired;

        AddDomainEvent(new PermitsExpiredEvent(
            EventId: new PermitsEventId(Uuid7.New()),
            PermitsId: PermitsId,
            ExpiredAtUtc: expiredAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Close / Archive -------------------------------------------

    /// <summary>
    /// Formally closes a completed permit (Completed -&gt; Closed). Valid only from
    /// <see cref="PermitsStatus.Completed"/>.
    /// </summary>
    public void Close(
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PermitsLifecycleRules.CanClose, PermitsStatus.Completed);

        ClosedAtUtc = closedAtUtc;
        Status = PermitsStatus.Closed;

        AddDomainEvent(new PermitsClosedEvent(
            EventId: new PermitsEventId(Uuid7.New()),
            PermitsId: PermitsId,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Archives a closed permit for compliance retention (Closed -&gt; Archived). Valid only from
    /// <see cref="PermitsStatus.Closed"/>. See <see cref="Policies.PermitsArchivalPolicy"/> for
    /// eligibility outside the aggregate boundary (e.g. batch archival jobs).
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PermitsLifecycleRules.CanArchive, PermitsStatus.Closed);

        ArchivedAtUtc = archivedAtUtc;
        Status = PermitsStatus.Archived;

        AddDomainEvent(new PermitsArchivedEvent(
            EventId: new PermitsEventId(Uuid7.New()),
            PermitsId: PermitsId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPermitsDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PermitsStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Permits {PermitsId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<PermitsStatus, bool> guard, PermitsStatus expected)
    {
        if (guard(Status)) return;

        throw new PermitsInvalidStatusTransitionException(
            PermitsId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
