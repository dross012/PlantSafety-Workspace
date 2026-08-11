// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.WorkOrderPriority
//  Layer:          Domain
//  File:           WorkOrderPriority.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the WorkOrderPriority bounded context (module: Scheduling).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.WorkOrderPriority.Aggregates;

/// <summary>Aggregate root for the WorkOrderPriority bounded context.</summary>
public sealed partial class WorkOrderPriority
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this WorkOrderPriority aggregate.</summary>
    public WorkOrderPriorityId WorkOrderPriorityId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public WorkOrderPriorityStatus Status { get; private set; }

    // ---- WorkOrderPriority-specific business properties --------------------------------

    /// <summary>PriorityCode business attribute.</summary>
    public string PriorityCode { get; private set; }

    /// <summary>HazardDescription business attribute.</summary>
    public string HazardDescription { get; private set; }

    /// <summary>DueByUtc business attribute.</summary>
    public UtcTimestamp? DueByUtc { get; private set; }

    /// <summary>EscalatedAtUtc business attribute.</summary>
    public UtcTimestamp? EscalatedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IWorkOrderPriorityDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IWorkOrderPriorityDomainEvent> DomainEvents
        => new ReadOnlyCollection<IWorkOrderPriorityDomainEvent>(_domainEvents);

    // For rehydration only.
    private WorkOrderPriority() 
    {
        // Assigned by the data mapper immediately after construction; see the public
        // constructor for the invariant these properties actually hold.
        HazardDescription = null!;
        PriorityCode = null!;
    }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new WorkOrderPriority aggregate and raises <see cref="WorkOrderPriorityCreatedEvent"/>.</summary>
    public static WorkOrderPriority Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string priorityCode = "",
        string hazardDescription = "",
        UtcTimestamp? dueByUtc = null,
        UtcTimestamp? escalatedAtUtc = null)
    {
        var agg = new WorkOrderPriority
        {
            WorkOrderPriorityId     = WorkOrderPriorityId.New(),
            TenantId      = tenantId,
            SiteId        = siteId,
            Status        = WorkOrderPriorityStatus.Open,
            PriorityCode = (priorityCode ?? string.Empty).Trim(),
            HazardDescription = (hazardDescription ?? string.Empty).Trim(),
            DueByUtc = dueByUtc,
            EscalatedAtUtc = escalatedAtUtc,
            CreatedAtUtc  = createdAtUtc,
            CreatedBy     = createdBy,
        };

        agg.AddDomainEvent(new WorkOrderPriorityCreatedEvent(
            EventId:       new WorkOrderPriorityEventId(Uuid7.New()),
            WorkOrderPriorityId:      agg.WorkOrderPriorityId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    /// <summary>Free-text notes recorded when the hazard is resolved.</summary>
    public string? ResolutionNotes { get; private set; }

    /// <summary>Response window, in hours, associated with <see cref="PriorityCode"/> (P1=immediate, P2=24-72h, P3=7-30 days, P4=next shutdown window).</summary>
    public int? ResponseWindowHours =>
        PriorityCode switch
        {
            "P1" => 0,
            "P2" => 72,
            "P3" => 720,
            "P4" => null,
            _ => null,
        };

    // ---- Lifecycle: Work Order Priorities (P1-P4) ----------------------------

    /// <summary>Begins work against the prioritized work order. Valid only from <see cref="WorkOrderPriorityStatus.Open"/>.</summary>
    public void StartWork(UtcTimestamp startedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, WorkOrderPriorityStatus.Open);

        Status = WorkOrderPriorityStatus.InProgress;

        AddDomainEvent(new WorkOrderPriorityStartedEvent(
            EventId: new WorkOrderPriorityEventId(Uuid7.New()),
            WorkOrderPriorityId: WorkOrderPriorityId,
            StartedAtUtc: startedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Escalates the work order after it passes its priority-based response window unresolved
    /// ("Automatic escalation for overdue P1/P2 items"). Valid from <see cref="WorkOrderPriorityStatus.Open"/>
    /// or <see cref="WorkOrderPriorityStatus.InProgress"/>.
    /// </summary>
    public void Escalate(string escalationReason, UtcTimestamp escalatedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, WorkOrderPriorityStatus.Open, WorkOrderPriorityStatus.InProgress);
        if (string.IsNullOrWhiteSpace(escalationReason))
            throw new ArgumentException("An escalation reason must be supplied.", nameof(escalationReason));

        EscalatedAtUtc = escalatedAtUtc;
        Status = WorkOrderPriorityStatus.Escalated;

        AddDomainEvent(new WorkOrderPriorityEscalatedEvent(
            EventId: new WorkOrderPriorityEventId(Uuid7.New()),
            WorkOrderPriorityId: WorkOrderPriorityId,
            PriorityCode: PriorityCode,
            EscalationReason: escalationReason.Trim(),
            EscalatedAtUtc: escalatedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Resolves the hazard addressed by the work order. Valid from <see cref="WorkOrderPriorityStatus.InProgress"/> or <see cref="WorkOrderPriorityStatus.Escalated"/>.</summary>
    public void Resolve(string? resolutionNotes, UtcTimestamp resolvedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, WorkOrderPriorityStatus.InProgress, WorkOrderPriorityStatus.Escalated);

        ResolutionNotes = string.IsNullOrWhiteSpace(resolutionNotes) ? null : resolutionNotes.Trim();
        Status = WorkOrderPriorityStatus.Resolved;

        AddDomainEvent(new WorkOrderPriorityResolvedEvent(
            EventId: new WorkOrderPriorityEventId(Uuid7.New()),
            WorkOrderPriorityId: WorkOrderPriorityId,
            ResolutionNotes: ResolutionNotes,
            ResolvedAtUtc: resolvedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>Closes a resolved work order. Valid only from <see cref="WorkOrderPriorityStatus.Resolved"/>.</summary>
    public void Close(UtcTimestamp closedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, WorkOrderPriorityStatus.Resolved);

        Status = WorkOrderPriorityStatus.Closed;

        AddDomainEvent(new WorkOrderPriorityClosedEvent(
            EventId: new WorkOrderPriorityEventId(Uuid7.New()),
            WorkOrderPriorityId: WorkOrderPriorityId,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IWorkOrderPriorityDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params WorkOrderPriorityStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for WorkOrderPriority {WorkOrderPriorityId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, params WorkOrderPriorityStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new WorkOrderPriorityInvalidStatusTransitionException(
            WorkOrderPriorityId, Status.ToString(), string.Join("|", allowed.Select(a => a.ToString())),
            correlationId, causationId);
    }
}