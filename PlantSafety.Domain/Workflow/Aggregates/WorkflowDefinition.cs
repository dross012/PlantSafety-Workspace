// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowDefinition.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a reusable Workflow definition: an ordered graph of steps
//                  (approvals, escalations, notifications, automated decisions, and predictive
//                  recommendation surfacing) connected by optionally-guarded transitions (Phase
//                  5.1 workflow definitions). WorkflowInstance aggregates are started against a
//                  specific, activated WorkflowDefinition.
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

/// <summary>Aggregate root for a reusable Workflow definition (steps, transitions, assigned roles/actors, conditions).</summary>
public sealed partial class WorkflowDefinition
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this WorkflowDefinition aggregate.</summary>
    public WorkflowDefinitionId WorkflowDefinitionId { get; private set; }

    /// <summary>Tenant that owns this workflow definition.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this definition is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Category taxonomy of this workflow: Approval, Escalation, Notification, or MultiStepProcess.</summary>
    public WorkflowType WorkflowType { get; private set; } = WorkflowType.Default;

    /// <summary>Human-readable name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Free-form description of this workflow's purpose.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Monotonically increasing definition revision number, starting at 1.</summary>
    public int Version { get; private set; } = 1;

    /// <summary>Whether this definition is active and may have new instances started against it.</summary>
    public bool IsActive { get; private set; }

    private readonly List<WorkflowStepDefinition> _steps = [];

    /// <summary>Ordered step graph nodes.</summary>
    public IReadOnlyList<WorkflowStepDefinition> Steps => _steps.AsReadOnly();

    private readonly List<WorkflowTransition> _transitions = [];

    /// <summary>Directed, optionally-guarded edges connecting steps.</summary>
    public IReadOnlyList<WorkflowTransition> Transitions => _transitions.AsReadOnly();

    // ---- Audit ---------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IWorkflowDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IWorkflowDomainEvent> DomainEvents
        => new ReadOnlyCollection<IWorkflowDomainEvent>(_domainEvents);

    // For rehydration only.
    private WorkflowDefinition() { }

    // ---- Factory -------------------------------------------------------------

    /// <summary>Creates a new, inactive WorkflowDefinition and raises <see cref="WorkflowDefinitionCreatedEvent"/>.</summary>
    public static WorkflowDefinition Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        WorkflowType workflowType,
        string name,
        string description)
    {
        var agg = new WorkflowDefinition
        {
            WorkflowDefinitionId = WorkflowDefinitionId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            WorkflowType = workflowType,
            Name = (name ?? string.Empty).Trim(),
            Description = (description ?? string.Empty).Trim(),
            Version = 1,
            IsActive = false,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new WorkflowDefinitionCreatedEvent(
            WorkflowEventId.New(), agg.WorkflowDefinitionId, workflowType.Code, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Graph construction ----------------------------------------------------

    /// <summary>
    /// Adds a step to the definition's step graph. Steps may only be added while the definition is
    /// inactive (draft) -- once <see cref="Activate"/> is called the graph is immutable.
    /// </summary>
    public WorkflowStepDefinition AddStep(
        int order,
        string name,
        WorkflowStepType stepType,
        AssignedActor assignedActor,
        AssignedActor escalationActor,
        GuardCondition entryCondition,
        TimeoutPolicy? timeout,
        RetryPolicy? retry,
        UtcTimestamp nowUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureIsDraft(correlationId, causationId);

        var step = new WorkflowStepDefinition(
            WorkflowDefinitionId, order, name, stepType, assignedActor, escalationActor, entryCondition, timeout, retry);
        _steps.Add(step);

        AddDomainEvent(new WorkflowStepDefinitionAddedEvent(
            WorkflowEventId.New(), WorkflowDefinitionId, step.StepId, order, nowUtc, actor, correlationId, causationId));

        return step;
    }

    /// <summary>Adds a directed transition between two steps already present in this definition's graph.</summary>
    public void AddTransition(WorkflowStepId fromStepId, WorkflowStepId toStepId, GuardCondition guard, bool isDefault, CorrelationId correlationId, CausationId causationId)
    {
        EnsureIsDraft(correlationId, causationId);

        if (FindStep(fromStepId) is null)
            throw new WorkflowStepNotFoundException(fromStepId, correlationId, causationId);
        if (FindStep(toStepId) is null)
            throw new WorkflowStepNotFoundException(toStepId, correlationId, causationId);

        _transitions.Add(new WorkflowTransition(fromStepId, toStepId, guard, isDefault));
    }

    /// <summary>Activates the definition, making it eligible to have WorkflowInstances started against it.</summary>
    public void Activate(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (_steps.Count == 0)
            throw new InvalidOperationException($"WorkflowDefinition {WorkflowDefinitionId} must have at least one step before activation.");

        IsActive = true;

        AddDomainEvent(new WorkflowDefinitionActivatedEvent(
            WorkflowEventId.New(), WorkflowDefinitionId, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Deactivates the definition, preventing new instances from starting against it. Existing instances are unaffected.</summary>
    public void Deactivate(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        IsActive = false;

        AddDomainEvent(new WorkflowDefinitionDeactivatedEvent(
            WorkflowEventId.New(), WorkflowDefinitionId, nowUtc, actor, correlationId, causationId));
    }

    // ---- Graph queries ---------------------------------------------------------

    /// <summary>Returns the step with the lowest <see cref="WorkflowStepDefinition.Order"/>, i.e. the workflow's entry point.</summary>
    public WorkflowStepDefinition GetStartStep()
        => _steps.OrderBy(s => s.Order).First();

    /// <summary>Finds a step by identity, or <see langword="null"/> if not present.</summary>
    public WorkflowStepDefinition? FindStep(WorkflowStepId stepId)
        => _steps.FirstOrDefault(s => s.StepId == stepId);

    /// <summary>Returns all transitions whose source is <paramref name="fromStepId"/>.</summary>
    public IReadOnlyList<WorkflowTransition> GetOutgoingTransitions(WorkflowStepId fromStepId)
        => _transitions.Where(t => t.FromStepId == fromStepId).ToList();

    /// <summary>
    /// Deterministically resolves the next step out of <paramref name="fromStepId"/> given the
    /// instance's context data: the first guarded transition whose <see cref="GuardCondition"/> is
    /// satisfied wins; failing that, the single transition marked <see cref="WorkflowTransition.IsDefault"/>
    /// is used. Returns <see langword="null"/> when no step follows (i.e. the workflow completes).
    /// </summary>
    public WorkflowStepDefinition? ResolveNextStep(WorkflowStepId fromStepId, IReadOnlyDictionary<string, string> contextData)
    {
        var outgoing = GetOutgoingTransitions(fromStepId);

        var satisfied = outgoing.FirstOrDefault(t => !t.IsDefault && t.Guard.Evaluate(contextData));
        if (satisfied != default)
            return FindStep(satisfied.ToStepId);

        var fallback = outgoing.FirstOrDefault(t => t.IsDefault);
        return fallback != default ? FindStep(fallback.ToStepId) : null;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IWorkflowDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureIsDraft(CorrelationId correlationId, CausationId causationId)
    {
        if (IsActive)
            throw new InvalidOperationException(
                $"WorkflowDefinition {WorkflowDefinitionId} is active; its step graph is immutable. " +
                $"CorrelationId={correlationId}, CausationId={causationId}");
    }
}
