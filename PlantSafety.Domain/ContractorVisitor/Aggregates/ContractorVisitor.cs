// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorVisitor
//  Layer:          Domain
//  File:           ContractorVisitor.cs
//  Created:        2026-06-17
//  Updated:        2026-08-02 -- Phase 4 full build-out: extended from identity-only scaffold with
//                  TenantId/SiteId tenant-scoping, contractor/visitor site-access fields
//                  (FullName/CompanyName/VisitorType/PurposeOfVisit/HostUserId/ScheduledArrivalUtc),
//                  the full Created->Active->Completed->Closed->Archived lifecycle, and a same-status
//                  CheckIn action (Active-scoped, sets ActualArrivalUtc). Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Aggregates.Auditing's build-out shape.
//                  Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the ContractorVisitor module -- tracks a single
//                  contractor/visitor site-access record from approval/scheduling through check-in
//                  and closeout.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorVisitor.Aggregates;

/// <summary>Aggregate root for the ContractorVisitor bounded context.</summary>
public sealed class ContractorVisitor
{
    /// <summary>Unique identity of this ContractorVisitor aggregate.</summary>
    public ContractorVisitorId ContractorVisitorId { get; private set; }

    /// <summary>Tenant that owns this contractor/visitor record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ContractorVisitorStatus Status { get; private set; }

    /// <summary>Full name of the contractor or visitor.</summary>
    public string FullName { get; private set; } = string.Empty;

    /// <summary>Company name the contractor or visitor represents.</summary>
    public string CompanyName { get; private set; } = string.Empty;

    /// <summary>Visitor classification: Contractor or Visitor.</summary>
    public string VisitorType { get; private set; } = "Visitor";

    /// <summary>Narrative purpose of the visit.</summary>
    public string PurposeOfVisit { get; private set; } = string.Empty;

    /// <summary>Identity of the host user sponsoring this visit.</summary>
    public UserId HostUserId { get; private set; }

    /// <summary>Scheduled arrival timestamp, if planned.</summary>
    public UtcTimestamp? ScheduledArrivalUtc { get; private set; }

    /// <summary>Actual arrival timestamp, populated on check-in.</summary>
    public UtcTimestamp? ActualArrivalUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // Domain events
    private readonly List<IContractorVisitorDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IContractorVisitorDomainEvent> DomainEvents
        => new ReadOnlyCollection<IContractorVisitorDomainEvent>(_domainEvents);

    private ContractorVisitor() { }

    /// <summary>Creates a new ContractorVisitor aggregate and raises <see cref="ContractorVisitorCreatedEvent"/>.</summary>
    /// <param name="createdBy">CRUD authorship identity.</param>
    /// <param name="createdAtUtc">Creation timestamp (UTC).</param>
    /// <param name="correlationId">Correlation lineage identifier.</param>
    /// <param name="causationId">Causation lineage identifier.</param>
    /// <param name="actor">Domain-action actor identity.</param>
    /// <param name="tenantId">Owning tenant.</param>
    /// <param name="siteId">Optional owning site.</param>
    /// <param name="fullName">Full name of the contractor or visitor.</param>
    /// <param name="companyName">Company name represented.</param>
    /// <param name="visitorType">Visitor classification (Contractor/Visitor).</param>
    /// <param name="purposeOfVisit">Narrative purpose of the visit.</param>
    /// <param name="hostUserId">Identity of the host user sponsoring this visit.</param>
    /// <param name="scheduledArrivalUtc">Optional scheduled arrival timestamp.</param>
    public static ContractorVisitor Create(
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string fullName = "",
        string companyName = "",
        string visitorType = "Visitor",
        string purposeOfVisit = "",
        UserId hostUserId = default,
        UtcTimestamp? scheduledArrivalUtc = null)
    {
        var agg = new ContractorVisitor
        {
            ContractorVisitorId     = ContractorVisitorId.New(),
            TenantId                = tenantId,
            SiteId                  = siteId,
            Status                  = ContractorVisitorStatus.Created,
            FullName                = (fullName ?? string.Empty).Trim(),
            CompanyName             = (companyName ?? string.Empty).Trim(),
            VisitorType             = (visitorType ?? "Visitor").Trim(),
            PurposeOfVisit          = (purposeOfVisit ?? string.Empty).Trim(),
            HostUserId              = hostUserId,
            ScheduledArrivalUtc     = scheduledArrivalUtc,
            CreatedAtUtc            = createdAtUtc,
            CreatedBy               = createdBy,
        };

        agg._domainEvents.Add(new ContractorVisitorCreatedEvent(
            EventId:       Uuid7.New(),
            ContractorVisitorId:      agg.ContractorVisitorId.Value,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: Activate / Complete / Close / Archive -------------------

    /// <summary>Activates the contractor/visitor record -- approved/scheduled (Created -&gt; Active). Valid only from <see cref="ContractorVisitorStatus.Created"/>.</summary>
    public void Activate(
        UserId actorUserId, UtcTimestamp atUtc, PerformedByActor actor,
        CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ContractorVisitorLifecycleRules.CanActivate, ContractorVisitorStatus.Created);

        Status = ContractorVisitorStatus.Active;

        _domainEvents.Add(new ContractorVisitorActivatedEvent(
            EventId: Uuid7.New(),
            ContractorVisitorId: ContractorVisitorId.Value,
            PerformedBy: actorUserId.Value,
            OccurredAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Records the contractor/visitor's actual arrival while the record is Active. A same-status
    /// action -- does not transition <see cref="Status"/>. Valid only while <see cref="ContractorVisitorStatus.Active"/>.
    /// </summary>
    public void CheckIn(
        UserId checkedInBy, UtcTimestamp checkedInAtUtc, PerformedByActor actor,
        CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, s => s == ContractorVisitorStatus.Active, ContractorVisitorStatus.Active);

        ActualArrivalUtc = checkedInAtUtc;

        _domainEvents.Add(new ContractorVisitorCheckedInEvent(
            EventId: Uuid7.New(),
            ContractorVisitorId: ContractorVisitorId.Value,
            PerformedBy: checkedInBy.Value,
            OccurredAtUtc: checkedInAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Completes the contractor/visitor record (Active -&gt; Completed). Valid only from <see cref="ContractorVisitorStatus.Active"/>.</summary>
    public void Complete(
        UserId actorUserId, UtcTimestamp atUtc, PerformedByActor actor,
        CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ContractorVisitorLifecycleRules.CanComplete, ContractorVisitorStatus.Active);

        Status = ContractorVisitorStatus.Completed;

        _domainEvents.Add(new ContractorVisitorCompletedEvent(
            EventId: Uuid7.New(),
            ContractorVisitorId: ContractorVisitorId.Value,
            PerformedBy: actorUserId.Value,
            OccurredAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Closes the contractor/visitor record (Completed -&gt; Closed). Valid only from <see cref="ContractorVisitorStatus.Completed"/>.</summary>
    public void Close(
        UserId actorUserId, UtcTimestamp atUtc, PerformedByActor actor,
        CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ContractorVisitorLifecycleRules.CanClose, ContractorVisitorStatus.Completed);

        Status = ContractorVisitorStatus.Closed;

        _domainEvents.Add(new ContractorVisitorClosedEvent(
            EventId: Uuid7.New(),
            ContractorVisitorId: ContractorVisitorId.Value,
            PerformedBy: actorUserId.Value,
            OccurredAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Archives the contractor/visitor record (Closed -&gt; Archived). Valid only from <see cref="ContractorVisitorStatus.Closed"/>.</summary>
    public void Archive(
        UserId actorUserId, UtcTimestamp atUtc, PerformedByActor actor,
        CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ContractorVisitorLifecycleRules.CanArchive, ContractorVisitorStatus.Closed);

        Status = ContractorVisitorStatus.Archived;

        _domainEvents.Add(new ContractorVisitorArchivedEvent(
            EventId: Uuid7.New(),
            ContractorVisitorId: ContractorVisitorId.Value,
            PerformedBy: actorUserId.Value,
            OccurredAtUtc: atUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<ContractorVisitorStatus, bool> guard, ContractorVisitorStatus expected)
    {
        if (guard(Status)) return;

        throw new ContractorVisitorInvalidStatusTransitionException(
            ContractorVisitorId.Value, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
