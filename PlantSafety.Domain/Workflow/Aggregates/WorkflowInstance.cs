// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowInstance.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a running (or completed) execution of a WorkflowDefinition.
//                  Implements the deterministic workflow runtime state machine (Phase 5.2):
//                  explicit WorkflowState/WorkflowStepStatus states, guarded transitions, bounded
//                  retry with deterministic backoff, and step timeout/escalation. Cross-aggregate
//                  navigation of the WorkflowDefinition's step graph is performed by
//                  Services.WorkflowRuntimeService, which calls the primitive mutators exposed
//                  here -- this aggregate never holds a reference to WorkflowDefinition.
//                  Additive-only evolution.
// ============================================================================
using System.Collections.ObjectModel;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.Entities;
using PlantSafety.Domain.Workflow.Enums;
using PlantSafety.Domain.Workflow.Events;
using PlantSafety.Domain.Workflow.Exceptions;
using PlantSafety.Domain.Workflow.ValueObjects;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;

namespace PlantSafety.Domain.Workflow.Aggregates;

/// <summary>Aggregate root for a running or completed execution of a WorkflowDefinition.</summary>
public sealed partial class WorkflowInstance
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this WorkflowInstance aggregate.</summary>
    public WorkflowInstanceId WorkflowInstanceId { get; private set; }

    /// <summary>Identity of the WorkflowDefinition this instance was started from.</summary>
    public WorkflowDefinitionId WorkflowDefinitionId { get; private set; }

    /// <summary>Tenant that owns this workflow instance.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this instance is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Category taxonomy inherited from the originating definition.</summary>
    public WorkflowType WorkflowType { get; private set; } = WorkflowType.Default;

    /// <summary>Overall lifecycle state of this instance (explicit finite state machine).</summary>
    public WorkflowState State { get; private set; } = WorkflowState.Default;

    /// <summary>Identity of the currently active step, or <see langword="null"/> before start / after completion.</summary>
    public WorkflowStepId? CurrentStepId { get; private set; }

    private readonly List<WorkflowStepInstance> _stepHistory = [];

    /// <summary>Full execution history of every step activated for this instance, in activation order.</summary>
    public IReadOnlyList<WorkflowStepInstance> StepHistory => _stepHistory.AsReadOnly();

    private readonly Dictionary<string, string> _contextData = [];

    /// <summary>
    /// Deterministic key/value context used to evaluate <see cref="ValueObjects.GuardCondition"/>
    /// transition guards. Populated at start and extended by automated-decision/recommendation
    /// steps as the instance progresses.
    /// </summary>
    public IReadOnlyDictionary<string, string> ContextData => new ReadOnlyDictionary<string, string>(_contextData);

    // ---- Timing ---------------------------------------------------------------

    /// <summary>UTC instant this instance was started.</summary>
    public UtcTimestamp StartedAtUtc { get; private set; }

    /// <summary>UTC instant this instance reached a terminal state, if any.</summary>
    public UtcTimestamp? CompletedAtUtc { get; private set; }

    // ---- Audit ---------------------------------------------------------------

    /// <summary>Identity of the user who initiated this instance.</summary>
    public UserId InitiatedBy { get; private set; }

    /// <summary>Correlation identifier propagated to every event and command raised for this instance.</summary>
    public Guid CorrelationId { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IWorkflowDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IWorkflowDomainEvent> DomainEvents
        => new ReadOnlyCollection<IWorkflowDomainEvent>(_domainEvents);

    // For rehydration only.
    private WorkflowInstance() { }

    // ---- Factory: Start ------------------------------------------------------

    /// <summary>
    /// Starts a new WorkflowInstance and activates its first step. Raises
    /// <see cref="WorkflowStartedEvent"/> and <see cref="WorkflowStepAdvancedEvent"/>.
    /// </summary>
    public static WorkflowInstance Start(
        WorkflowDefinitionId workflowDefinitionId,
        WorkflowType workflowType,
        WorkflowStepId firstStepId,
        WorkflowStepType firstStepType,
        AssignedActor firstStepAssignedActor,
        UserId initiatedBy,
        UtcTimestamp nowUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        IReadOnlyDictionary<string, string>? initialContextData = null)
    {
        var agg = new WorkflowInstance
        {
            WorkflowInstanceId = WorkflowInstanceId.New(),
            WorkflowDefinitionId = workflowDefinitionId,
            TenantId = tenantId,
            SiteId = siteId,
            WorkflowType = workflowType,
            State = WorkflowState.InProgress,
            StartedAtUtc = nowUtc,
            InitiatedBy = initiatedBy,
            CorrelationId = correlationId.Value,
        };

        if (initialContextData is not null)
            foreach (var kvp in initialContextData)
                agg._contextData[kvp.Key] = kvp.Value;

        agg.AddDomainEvent(new WorkflowStartedEvent(
            WorkflowEventId.New(), agg.WorkflowInstanceId, workflowDefinitionId, nowUtc, actor, correlationId, causationId));

        agg.ActivateStep(firstStepId, firstStepType, firstStepAssignedActor, fromStepId: null, nowUtc, actor, correlationId, causationId);

        return agg;
    }

    // ---- Runtime: step activation / advancement --------------------------------

    /// <summary>
    /// Activates <paramref name="stepId"/> as the new current step, appending a new
    /// <see cref="WorkflowStepInstance"/> to <see cref="StepHistory"/>. Called both for the initial
    /// step (by <see cref="Start"/>) and for subsequent transitions (by
    /// Services.WorkflowRuntimeService, after it resolves the next step from the
    /// WorkflowDefinition's guarded transition graph).
    /// </summary>
    public void ActivateStep(
        WorkflowStepId stepId,
        WorkflowStepType stepType,
        AssignedActor assignedActor,
        WorkflowStepId? fromStepId,
        UtcTimestamp nowUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureNotTerminal(correlationId, causationId);

        var stepInstance = new WorkflowStepInstance(WorkflowInstanceId, stepId, stepType, assignedActor);
        stepInstance.Activate(nowUtc);
        _stepHistory.Add(stepInstance);
        CurrentStepId = stepId;

        State = stepType.Code switch
        {
            "HumanApproval" => WorkflowState.WaitingForApproval,
            _ => WorkflowState.InProgress,
        };

        AddDomainEvent(new WorkflowStepAdvancedEvent(
            WorkflowEventId.New(), WorkflowInstanceId, fromStepId, stepId, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Returns the <see cref="WorkflowStepInstance"/> record for the currently active step.</summary>
    public WorkflowStepInstance GetCurrentStepInstance(CorrelationId correlationId, CausationId causationId)
        => _stepHistory.LastOrDefault(s => s.StepId == CurrentStepId && s.Status is WorkflowStepStatus.Active or WorkflowStepStatus.Escalated or WorkflowStepStatus.Retrying)
           ?? throw new WorkflowStepAlreadyDecidedException(WorkflowStepInstanceId.New(), correlationId, causationId);

    /// <summary>Marks the current step complete with no decision to record (e.g. a Notification step) and clears <see cref="CurrentStepId"/> pending the runtime service's next-step resolution.</summary>
    public void CompleteCurrentStep(UtcTimestamp nowUtc)
    {
        var current = _stepHistory.LastOrDefault(s => s.StepId == CurrentStepId);
        current?.Complete(nowUtc);
    }

    /// <summary>Records that the given context key should carry the given value, for use by subsequent guard-condition evaluation.</summary>
    public void SetContextValue(string key, string value) => _contextData[key.Trim()] = value ?? string.Empty;

    // ---- Runtime: human approval -----------------------------------------------

    /// <summary>Records a human approval decision on the currently active HumanApproval step.</summary>
    public void ApproveCurrentStep(UserId approvedBy, string? notes, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        var step = GetCurrentStepInstance(correlationId, causationId);
        EnsureStepIsActionable(step, correlationId, causationId);

        step.Approve(approvedBy, notes, nowUtc);
        State = WorkflowState.InProgress;

        AddDomainEvent(new WorkflowApprovedEvent(
            WorkflowEventId.New(), WorkflowInstanceId, step.StepId, approvedBy, notes, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Records a human rejection decision on the currently active HumanApproval step. Terminal: the instance transitions to Rejected.</summary>
    public void RejectCurrentStep(UserId rejectedBy, string? reason, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        var step = GetCurrentStepInstance(correlationId, causationId);
        EnsureStepIsActionable(step, correlationId, causationId);

        step.Reject(rejectedBy, reason, nowUtc);
        State = WorkflowState.Rejected;
        CompletedAtUtc = nowUtc;

        AddDomainEvent(new WorkflowRejectedEvent(
            WorkflowEventId.New(), WorkflowInstanceId, step.StepId, rejectedBy, reason, nowUtc, actor, correlationId, causationId));
    }

    // ---- Runtime: automated decision / predictive recommendation ---------------

    /// <summary>Records an automated-decision outcome resolved by a domain policy/rule without human input.</summary>
    public void RecordAutomatedDecision(bool approved, string? notes, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        var step = GetCurrentStepInstance(correlationId, causationId);
        EnsureStepIsActionable(step, correlationId, causationId);

        step.RecordAutomatedDecision(approved, notes, nowUtc);
        State = WorkflowState.InProgress;

        AddDomainEvent(new WorkflowAutomatedDecisionMadeEvent(
            WorkflowEventId.New(), WorkflowInstanceId, step.StepId, approved, notes, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>
    /// Attaches a predictive recommendation (sourced from a predictive-analytics module via a
    /// SharedKernel/integration-event contract, never a direct Domain reference) to the currently
    /// active PredictiveRecommendation step.
    /// </summary>
    public void SurfaceRecommendation(string recommendationReference, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (string.IsNullOrWhiteSpace(recommendationReference))
            throw new ArgumentException("Recommendation reference must not be empty.", nameof(recommendationReference));

        var step = GetCurrentStepInstance(correlationId, causationId);
        EnsureStepIsActionable(step, correlationId, causationId);

        step.SurfaceRecommendation(recommendationReference, nowUtc);
        State = WorkflowState.InProgress;

        AddDomainEvent(new WorkflowRecommendationSurfacedEvent(
            WorkflowEventId.New(), WorkflowInstanceId, step.StepId, recommendationReference, nowUtc, actor, correlationId, causationId));
    }

    // ---- Runtime: timeout / escalation / retry ----------------------------------

    /// <summary>Marks the currently active step as timed out, per <see cref="TimeoutPolicy"/>.</summary>
    public void TimeoutCurrentStep(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        var step = GetCurrentStepInstance(correlationId, causationId);
        step.MarkTimedOut();
        State = WorkflowState.TimedOut;

        AddDomainEvent(new WorkflowTimedOutEvent(
            WorkflowEventId.New(), WorkflowInstanceId, step.StepId, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Escalates the currently timed-out step to <paramref name="escalationActor"/>.</summary>
    public void EscalateCurrentStep(AssignedActor escalationActor, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        var step = _stepHistory.LastOrDefault(s => s.StepId == CurrentStepId)
            ?? throw new WorkflowStepAlreadyDecidedException(WorkflowStepInstanceId.New(), correlationId, causationId);

        step.MarkEscalated(escalationActor);
        State = WorkflowState.Escalated;

        AddDomainEvent(new WorkflowEscalatedEvent(
            WorkflowEventId.New(), WorkflowInstanceId, step.StepId, escalationActor.ToString(), nowUtc, actor, correlationId, causationId));
    }

    /// <summary>
    /// Schedules a bounded retry of the currently active step using <paramref name="retryPolicy"/>.
    /// Deterministic: never sleeps -- computes <see cref="WorkflowStepInstance.NextAttemptAtUtc"/>
    /// from <paramref name="nowUtc"/> via <see cref="RetryPolicy.ComputeNextAttemptUtc"/>.
    /// </summary>
    public void ScheduleRetry(RetryPolicy retryPolicy, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        var step = _stepHistory.LastOrDefault(s => s.StepId == CurrentStepId)
            ?? throw new WorkflowStepAlreadyDecidedException(WorkflowStepInstanceId.New(), correlationId, causationId);

        if (!retryPolicy.CanRetry(step.AttemptCount))
        {
            AddDomainEvent(new WorkflowRetryLimitExceededEvent(
                WorkflowEventId.New(), WorkflowInstanceId, step.StepId, step.AttemptCount, nowUtc, actor, correlationId, causationId));

            State = WorkflowState.Failed;
            CompletedAtUtc = nowUtc;

            AddDomainEvent(new WorkflowFailedEvent(
                WorkflowEventId.New(), WorkflowInstanceId, step.StepId,
                $"Retry budget of {retryPolicy.MaxAttempts} exhausted.", nowUtc, actor, correlationId, causationId));

            return;
        }

        var nextAttemptNumber = step.AttemptCount + 1;
        var nextAttemptAtUtc = retryPolicy.ComputeNextAttemptUtc(nextAttemptNumber, nowUtc);
        step.ScheduleRetry(nextAttemptAtUtc);
        State = WorkflowState.Retrying;

        AddDomainEvent(new WorkflowRetryScheduledEvent(
            WorkflowEventId.New(), WorkflowInstanceId, step.StepId, nextAttemptNumber, nextAttemptAtUtc, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Re-activates the currently retrying step for another attempt (transitions Retrying -> Active).</summary>
    public void BeginRetryAttempt(UtcTimestamp nowUtc)
    {
        var step = _stepHistory.LastOrDefault(s => s.StepId == CurrentStepId);
        step?.Activate(nowUtc);
        State = WorkflowState.InProgress;
    }

    // ---- Runtime: completion / cancellation -------------------------------------

    /// <summary>Marks the instance as completed once no further steps remain to be activated.</summary>
    public void Complete(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureNotTerminal(correlationId, causationId);

        State = WorkflowState.Completed;
        CompletedAtUtc = nowUtc;
        CurrentStepId = null;

        AddDomainEvent(new WorkflowCompletedEvent(
            WorkflowEventId.New(), WorkflowInstanceId, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Cancels the instance before completion.</summary>
    public void Cancel(string? reason, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureNotTerminal(correlationId, causationId);

        var current = _stepHistory.LastOrDefault(s => s.StepId == CurrentStepId);
        current?.Cancel();

        State = WorkflowState.Cancelled;
        CompletedAtUtc = nowUtc;

        AddDomainEvent(new WorkflowCancelledEvent(
            WorkflowEventId.New(), WorkflowInstanceId, reason, nowUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IWorkflowDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureNotTerminal(CorrelationId correlationId, CausationId causationId)
    {
        if (State.IsTerminal)
            throw new WorkflowInvalidStateTransitionException(WorkflowInstanceId, State.Code, "(non-terminal)", correlationId, causationId);
    }

    private static void EnsureStepIsActionable(WorkflowStepInstance step, CorrelationId correlationId, CausationId causationId)
    {
        if (step.Status is not (WorkflowStepStatus.Active or WorkflowStepStatus.Retrying))
            throw new WorkflowStepAlreadyDecidedException(step.StepInstanceId, correlationId, causationId);
    }
}
