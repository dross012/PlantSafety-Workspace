// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           OptimizationRun.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root representing a single invocation of the predictive scheduling
//                  optimization pipeline (Application.Scheduling.PredictiveScheduling.Optimization's
//                  IOptimizationStrategy set, selected via MultiObjectiveScheduleOptimizer). Captures
//                  the input SchedulableTask/ResourceAvailabilityWindow snapshot, the requested
//                  OptimizationObjective, and -- once the run completes -- the winning strategy's
//                  name, resulting ScheduleAssignments, tolerated ConstraintViolations, and any
//                  unscheduled tasks. Gives past optimization/reprioritization decisions a durable,
//                  queryable, auditable record where previously they were computed and discarded.
//                  Explicit finite state machine (Pending -&gt; Running -&gt; Completed|Failed), guarded
//                  by OptimizationRunLifecycleRules, mirroring Workflow.Aggregates.WorkflowInstance's
//                  identical precedent. Additive-only evolution.
// ============================================================================
using System.Collections.ObjectModel;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.DomainEvents;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Enums;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Exceptions;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Rules;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects.Ids;

namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Aggregates;

/// <summary>Aggregate root for a single invocation of the predictive scheduling optimization pipeline.</summary>
public sealed partial class OptimizationRun
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this OptimizationRun aggregate.</summary>
    public OptimizationRunId OptimizationRunId { get; private set; }

    /// <summary>Tenant that owns this optimization run.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this run is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    // ---- Inputs (snapshot at request time) ------------------------------------

    private readonly List<SchedulableTask> _inputTasks = [];

    /// <summary>Immutable snapshot of the tasks competing for placement in this run.</summary>
    public IReadOnlyList<SchedulableTask> InputTasks => _inputTasks.AsReadOnly();

    private readonly List<ResourceAvailabilityWindow> _inputAvailability = [];

    /// <summary>Immutable snapshot of the resource availability windows supplied to this run.</summary>
    public IReadOnlyList<ResourceAvailabilityWindow> InputAvailability => _inputAvailability.AsReadOnly();

    /// <summary>Objective the optimization pipeline was requested to optimize for.</summary>
    public OptimizationObjective Objective { get; private set; }

    // ---- Outputs (populated on completion) -------------------------------------

    /// <summary>Name of the strategy selected as best-scoring for <see cref="Objective"/>, once completed.</summary>
    public string? SelectedStrategyName { get; private set; }

    private readonly List<ScheduleAssignment> _resultAssignments = [];

    /// <summary>Feasible task-to-resource assignments produced by the selected strategy.</summary>
    public IReadOnlyList<ScheduleAssignment> ResultAssignments => _resultAssignments.AsReadOnly();

    private readonly List<SchedulableTaskId> _unscheduledTaskIds = [];

    /// <summary>Identities of tasks the selected strategy was unable to place within a feasible window.</summary>
    public IReadOnlyList<SchedulableTaskId> UnscheduledTaskIds => _unscheduledTaskIds.AsReadOnly();

    private readonly List<ConstraintViolation> _toleratedViolations = [];

    /// <summary>Non-blocking (Warning-severity) violations tolerated by the accepted assignments.</summary>
    public IReadOnlyList<ConstraintViolation> ToleratedViolations => _toleratedViolations.AsReadOnly();

    /// <summary>Composite desirability score of the selected result, once completed.</summary>
    public double? Score { get; private set; }

    /// <summary>Explanation recorded when the run transitions to Failed.</summary>
    public string? FailureReason { get; private set; }

    // ---- Status / timing --------------------------------------------------------

    /// <summary>Current lifecycle status of this run.</summary>
    public OptimizationRunStatus Status { get; private set; } = OptimizationRunStatus.Pending;

    /// <summary>UTC instant this run was requested.</summary>
    public UtcTimestamp RequestedAtUtc { get; private set; }

    /// <summary>UTC instant the run reached a terminal status (Completed or Failed), if any.</summary>
    public UtcTimestamp? CompletedAtUtc { get; private set; }

    // ---- Audit ---------------------------------------------------------------

    /// <summary>Identity of the user (or null for a system-triggered run, e.g. reprioritization) who requested this run.</summary>
    public UserId? RequestedBy { get; private set; }

    /// <summary>Correlation identifier propagated to every event raised for this run.</summary>
    public Guid CorrelationId { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IOptimizationRunDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IOptimizationRunDomainEvent> DomainEvents
        => new ReadOnlyCollection<IOptimizationRunDomainEvent>(_domainEvents);

    // For rehydration only.
    private OptimizationRun() { }

    // ---- Factory: Request ------------------------------------------------------

    /// <summary>
    /// Creates a new OptimizationRun in the Pending status, capturing the input task/availability
    /// snapshot and requested objective. Does not itself invoke the optimization pipeline -- callers
    /// (Application layer) call <see cref="Begin"/> once execution starts and <see cref="Complete"/>
    /// or <see cref="Fail"/> once it finishes.
    /// </summary>
    public static OptimizationRun Request(
        Guid tenantId,
        Guid? siteId,
        IReadOnlyList<SchedulableTask> inputTasks,
        IReadOnlyList<ResourceAvailabilityWindow> inputAvailability,
        OptimizationObjective objective,
        UserId? requestedBy,
        UtcTimestamp nowUtc,
        CorrelationId correlationId)
    {
        ArgumentNullException.ThrowIfNull(inputTasks);
        ArgumentNullException.ThrowIfNull(inputAvailability);

        var agg = new OptimizationRun
        {
            OptimizationRunId = OptimizationRunId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Objective = objective,
            Status = OptimizationRunStatus.Pending,
            RequestedAtUtc = nowUtc,
            RequestedBy = requestedBy,
            CorrelationId = correlationId.Value,
        };

        agg._inputTasks.AddRange(inputTasks);
        agg._inputAvailability.AddRange(inputAvailability);

        return agg;
    }

    // ---- Runtime: lifecycle transitions -----------------------------------------

    /// <summary>Transitions the run from Pending to Running, raising <see cref="OptimizationRunStartedEvent"/>.</summary>
    public void Begin(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!OptimizationRunLifecycleRules.CanBegin(Status))
            throw new OptimizationRunInvalidStateTransitionException(OptimizationRunId, Status.ToString(), "Begin", correlationId, causationId);

        Status = OptimizationRunStatus.Running;

        AddDomainEvent(new OptimizationRunStartedEvent(
            OptimizationRunEventId.New(), OptimizationRunId, Objective, _inputTasks.Count, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>
    /// Transitions the run from Running to Completed, recording the winning strategy's result.
    /// Raises <see cref="OptimizationRunCompletedEvent"/>.
    /// </summary>
    public void Complete(
        string selectedStrategyName,
        IReadOnlyList<ScheduleAssignment> assignments,
        IReadOnlyList<SchedulableTaskId> unscheduledTaskIds,
        IReadOnlyList<ConstraintViolation> toleratedViolations,
        double score,
        UtcTimestamp nowUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (!OptimizationRunLifecycleRules.CanComplete(Status))
            throw new OptimizationRunInvalidStateTransitionException(OptimizationRunId, Status.ToString(), "Complete", correlationId, causationId);

        if (string.IsNullOrWhiteSpace(selectedStrategyName))
            throw new ArgumentException("OptimizationRun.Complete requires a non-empty selectedStrategyName.", nameof(selectedStrategyName));

        SelectedStrategyName = selectedStrategyName;
        _resultAssignments.Clear();
        _resultAssignments.AddRange(assignments ?? []);
        _unscheduledTaskIds.Clear();
        _unscheduledTaskIds.AddRange(unscheduledTaskIds ?? []);
        _toleratedViolations.Clear();
        _toleratedViolations.AddRange(toleratedViolations ?? []);
        Score = score;
        Status = OptimizationRunStatus.Completed;
        CompletedAtUtc = nowUtc;

        AddDomainEvent(new OptimizationRunCompletedEvent(
            OptimizationRunEventId.New(), OptimizationRunId, selectedStrategyName,
            _resultAssignments.Count, _unscheduledTaskIds.Count, _toleratedViolations.Count, score,
            nowUtc, actor, correlationId, causationId));
    }

    /// <summary>
    /// Transitions the run to Failed (from Pending or Running), recording a failure reason. Raises
    /// <see cref="OptimizationRunFailedEvent"/>.
    /// </summary>
    public void Fail(string reason, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!OptimizationRunLifecycleRules.CanFail(Status))
            throw new OptimizationRunInvalidStateTransitionException(OptimizationRunId, Status.ToString(), "Fail", correlationId, causationId);

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("OptimizationRun.Fail requires a non-empty reason.", nameof(reason));

        FailureReason = reason;
        Status = OptimizationRunStatus.Failed;
        CompletedAtUtc = nowUtc;

        AddDomainEvent(new OptimizationRunFailedEvent(
            OptimizationRunEventId.New(), OptimizationRunId, reason, nowUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IOptimizationRunDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
