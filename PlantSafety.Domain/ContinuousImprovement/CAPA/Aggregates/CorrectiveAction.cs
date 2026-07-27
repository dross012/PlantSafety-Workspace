// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.CAPA
//  Layer:          Domain
//  File:           CorrectiveAction.cs
//  Created:        2026-07-27
//  Author:         PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the ContinuousImprovement CAPA (Corrective and Preventive
//                  Action) bounded context, backing [ContinuousImprovement].[CorrectiveAction]
//                  (usp_CorrectiveAction_Create / usp_CorrectiveAction_GetById /
//                  usp_CorrectiveAction_List via ICorrectiveActionRepository /
//                  CorrectiveActionDapperRepository). Before this pass, this module contained only
//                  a "CAPAStub.cs" placeholder marker class ("Replace with the full aggregate
//                  structure") -- there was no domain model at all backing this feature; the
//                  Application-layer CreateCorrectiveActionCommandHandler pushed primitives
//                  straight to Dapper/SQL with only FluentValidation guarding required fields,
//                  and the already-scaffolded CorrectiveActionClosedEvent (Application-layer) was
//                  never raised by anything. Lifecycle states (Open/InProgress/AwaitingVerification/
//                  Closed/Overdue) are taken verbatim from the workflow-status vocabulary already
//                  documented on CorrectiveActionDto.Status -- no new states were invented; only
//                  Open is currently reachable via a real stored procedure
//                  (usp_CorrectiveAction_Create), so Start/Complete/Close/MarkOverdue are
//                  domain-complete and unit-tested here but are not yet wired to new stored
//                  procedures/commands (tracked follow-up -- see
//                  docs/architecture/plantsafety-architecture.md). Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ContinuousImprovement.CAPA.Enums;
using PlantSafety.Domain.ContinuousImprovement.CAPA.Events;
using PlantSafety.Domain.ContinuousImprovement.CAPA.Exceptions;
using PlantSafety.Domain.ContinuousImprovement.CAPA.ValueObjects.Ids;

namespace PlantSafety.Domain.ContinuousImprovement.CAPA.Aggregates;

/// <summary>Aggregate root for the ContinuousImprovement CAPA bounded context.</summary>
public sealed class CorrectiveAction
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this CorrectiveAction aggregate.</summary>
    public CorrectiveActionId CorrectiveActionId { get; private set; }

    /// <summary>Tenant that owns this CAPA record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public CorrectiveActionStatus Status { get; private set; }

    // ---- CAPA-specific business properties ------------------------------------

    /// <summary>Identity of the source safety event that triggered this CAPA (e.g. IncidentId, AuditFindingId, NearMissId).</summary>
    public Guid SourceEventId { get; private set; }

    /// <summary>Type discriminator of the source event (Incident, NearMiss, AuditFinding, Observation).</summary>
    public string SourceEventType { get; private set; } = string.Empty;

    /// <summary>Title of the corrective action.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Description of the corrective action.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Action type: Corrective (fixes a found problem) or Preventive (prevents recurrence).</summary>
    public string ActionType { get; private set; } = string.Empty;

    /// <summary>Priority (e.g. Low, Medium, High, Critical).</summary>
    public string Priority { get; private set; } = string.Empty;

    /// <summary>Worker assigned to implement this action.</summary>
    public UserId AssignedToWorkerId { get; private set; }

    /// <summary>UTC due date for completion.</summary>
    public UtcTimestamp DueAtUtc { get; private set; }

    /// <summary>UTC instant this action was marked Completed (moved to AwaitingVerification), if any.</summary>
    public UtcTimestamp? CompletedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<ICorrectiveActionDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ICorrectiveActionDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // For rehydration only.
    private CorrectiveAction() { }

    // ---- Factory ---------------------------------------------------------------

    /// <summary>
    /// Creates a new CorrectiveAction (CAPA) item and raises <see cref="CorrectiveActionCreatedEvent"/>.
    /// Enforces the required-field invariants implied by the NOT NULL columns on
    /// [ContinuousImprovement].[CorrectiveAction] (Title, Description, ActionType, Priority,
    /// SourceEventType, SourceEventId, AssignedToWorkerId).
    /// </summary>
    /// <exception cref="ArgumentException">A required field is missing or empty.</exception>
    public static CorrectiveAction Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid sourceEventId,
        string sourceEventType,
        string title,
        string description,
        string actionType,
        string priority,
        UserId assignedToWorkerId,
        UtcTimestamp dueAtUtc)
    {
        var normalizedTitle = (title ?? string.Empty).Trim();
        var normalizedDescription = (description ?? string.Empty).Trim();
        var normalizedActionType = (actionType ?? string.Empty).Trim();
        var normalizedPriority = (priority ?? string.Empty).Trim();
        var normalizedSourceEventType = (sourceEventType ?? string.Empty).Trim();

        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (sourceEventId == Guid.Empty)
            throw new ArgumentException("SourceEventId is required.", nameof(sourceEventId));
        if (string.IsNullOrWhiteSpace(normalizedSourceEventType))
            throw new ArgumentException("SourceEventType is required.", nameof(sourceEventType));
        if (string.IsNullOrWhiteSpace(normalizedTitle))
            throw new ArgumentException("Title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(normalizedDescription))
            throw new ArgumentException("Description is required.", nameof(description));
        if (string.IsNullOrWhiteSpace(normalizedActionType))
            throw new ArgumentException("ActionType is required.", nameof(actionType));
        if (string.IsNullOrWhiteSpace(normalizedPriority))
            throw new ArgumentException("Priority is required.", nameof(priority));
        if (assignedToWorkerId == default)
            throw new ArgumentException("AssignedToWorkerId is required.", nameof(assignedToWorkerId));

        var agg = new CorrectiveAction
        {
            CorrectiveActionId = CorrectiveActionId.New(),
            TenantId            = tenantId,
            Status              = CorrectiveActionStatus.Open,
            SourceEventId       = sourceEventId,
            SourceEventType     = normalizedSourceEventType,
            Title               = normalizedTitle,
            Description         = normalizedDescription,
            ActionType          = normalizedActionType,
            Priority            = normalizedPriority,
            AssignedToWorkerId  = assignedToWorkerId,
            DueAtUtc            = dueAtUtc,
            CreatedAtUtc        = createdAtUtc,
            CreatedBy           = createdBy,
        };

        agg.AddDomainEvent(new CorrectiveActionCreatedEvent(
            CorrectiveActionEventId.New(), agg.CorrectiveActionId, sourceEventId, normalizedSourceEventType,
            assignedToWorkerId, dueAtUtc, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Lifecycle: Start ------------------------------------------------------

    /// <summary>Transitions the action from Open to InProgress.</summary>
    /// <exception cref="CorrectiveActionInvalidStatusTransitionException">The action is not Open.</exception>
    public void Start(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != CorrectiveActionStatus.Open)
            throw new CorrectiveActionInvalidStatusTransitionException(
                CorrectiveActionId, Status.ToString(), CorrectiveActionStatus.InProgress.ToString(), correlationId, causationId);

        Status = CorrectiveActionStatus.InProgress;

        AddDomainEvent(new CorrectiveActionStartedEvent(
            CorrectiveActionEventId.New(), CorrectiveActionId, nowUtc, actor, correlationId, causationId));
    }

    // ---- Lifecycle: Complete ----------------------------------------------------

    /// <summary>Marks the action's work Completed (moves to AwaitingVerification) and records the completion timestamp.</summary>
    /// <exception cref="CorrectiveActionInvalidStatusTransitionException">The action is Closed or already awaiting verification.</exception>
    public void Complete(UtcTimestamp completedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status is not (CorrectiveActionStatus.Open or CorrectiveActionStatus.InProgress or CorrectiveActionStatus.Overdue))
            throw new CorrectiveActionInvalidStatusTransitionException(
                CorrectiveActionId, Status.ToString(), CorrectiveActionStatus.AwaitingVerification.ToString(), correlationId, causationId);

        Status = CorrectiveActionStatus.AwaitingVerification;
        CompletedAtUtc = completedAtUtc;

        AddDomainEvent(new CorrectiveActionCompletedEvent(
            CorrectiveActionEventId.New(), CorrectiveActionId, completedAtUtc, actor, correlationId, causationId));
    }

    // ---- Lifecycle: Close -------------------------------------------------------

    /// <summary>Verifies and formally closes the action (terminal state).</summary>
    /// <exception cref="CorrectiveActionInvalidStatusTransitionException">The action is not AwaitingVerification.</exception>
    public void Close(UtcTimestamp closedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != CorrectiveActionStatus.AwaitingVerification)
            throw new CorrectiveActionInvalidStatusTransitionException(
                CorrectiveActionId, Status.ToString(), CorrectiveActionStatus.Closed.ToString(), correlationId, causationId);

        Status = CorrectiveActionStatus.Closed;

        AddDomainEvent(new CorrectiveActionClosedEvent(
            CorrectiveActionEventId.New(), CorrectiveActionId, closedAtUtc, actor, correlationId, causationId));
    }

    // ---- Lifecycle: Overdue ------------------------------------------------------

    /// <summary>Marks the action Overdue when its due date has passed without completion. No-op from a completed/closed state.</summary>
    public void MarkOverdue(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status is not (CorrectiveActionStatus.Open or CorrectiveActionStatus.InProgress))
            return;

        Status = CorrectiveActionStatus.Overdue;

        AddDomainEvent(new CorrectiveActionMarkedOverdueEvent(
            CorrectiveActionEventId.New(), CorrectiveActionId, nowUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ICorrectiveActionDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
