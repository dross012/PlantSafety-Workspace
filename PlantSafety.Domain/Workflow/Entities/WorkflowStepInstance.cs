// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowStepInstance.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Child entity of the WorkflowInstance aggregate tracking the runtime execution
//                  record of a single step: status, activation/decision timestamps, retry attempt
//                  count, and (for predictive-recommendation steps) the surfaced recommendation
//                  reference (Phase 5.2 runtime; Phase 5.3 human + machine collaboration).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.Enums;
using PlantSafety.Domain.Workflow.ValueObjects;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;

namespace PlantSafety.Domain.Workflow.Entities;

/// <summary>Runtime execution record of a single step within a WorkflowInstance.</summary>
public sealed class WorkflowStepInstance
{
    /// <summary>Unique identity of this step-instance record.</summary>
    public WorkflowStepInstanceId StepInstanceId { get; private set; }

    /// <summary>Identity of the owning WorkflowInstance aggregate.</summary>
    public WorkflowInstanceId WorkflowInstanceId { get; private set; }

    /// <summary>Identity of the WorkflowStepDefinition this record executes.</summary>
    public WorkflowStepId StepId { get; private set; }

    /// <summary>How this step is resolved, copied from the definition at activation time.</summary>
    public WorkflowStepType StepType { get; private set; } = WorkflowStepType.Default;

    /// <summary>Current runtime status of this step.</summary>
    public WorkflowStepStatus Status { get; private set; } = WorkflowStepStatus.Pending;

    /// <summary>Actor assigned to resolve this step, copied from the definition at activation time.</summary>
    public AssignedActor AssignedActor { get; private set; }

    /// <summary>UTC instant this step became active, if it has been activated.</summary>
    public UtcTimestamp? ActivatedAtUtc { get; private set; }

    /// <summary>UTC instant a decision (approve/reject/automated outcome) was recorded, if any.</summary>
    public UtcTimestamp? DecidedAtUtc { get; private set; }

    /// <summary>Identity of the user who recorded the decision, if a human decision was made.</summary>
    public UserId? DecidedBy { get; private set; }

    /// <summary>Free-text notes captured with the decision, if any.</summary>
    public string? DecisionNotes { get; private set; }

    /// <summary>Number of retry attempts made so far for this step.</summary>
    public int AttemptCount { get; private set; }

    /// <summary>UTC instant the next retry attempt is scheduled for, if a retry is pending.</summary>
    public UtcTimestamp? NextAttemptAtUtc { get; private set; }

    /// <summary>
    /// Opaque reference to a recommendation surfaced from a predictive-analytics module (e.g. an
    /// Intelligence.PredictiveSafety alert id or Analytics.PredictiveAnalytics forecast id),
    /// carried purely as a string reference to avoid a Domain-to-Domain compile-time dependency.
    /// Populated only for <see cref="WorkflowStepType.PredictiveRecommendation"/> steps.
    /// </summary>
    public string? RecommendationReference { get; private set; }

    /// <summary>For ORM/reflection rehydration only.</summary>
    private WorkflowStepInstance() { }

    /// <summary>Creates a new, pending step-instance record.</summary>
    public WorkflowStepInstance(WorkflowInstanceId workflowInstanceId, WorkflowStepId stepId, WorkflowStepType stepType, AssignedActor assignedActor)
    {
        StepInstanceId = WorkflowStepInstanceId.New();
        WorkflowInstanceId = workflowInstanceId;
        StepId = stepId;
        StepType = stepType;
        AssignedActor = assignedActor;
        Status = WorkflowStepStatus.Pending;
    }

    /// <summary>Activates this step, marking it as the current step of the instance.</summary>
    public void Activate(UtcTimestamp nowUtc)
    {
        Status = WorkflowStepStatus.Active;
        ActivatedAtUtc = nowUtc;
    }

    /// <summary>Records a human approval decision.</summary>
    public void Approve(UserId decidedBy, string? notes, UtcTimestamp nowUtc)
    {
        Status = WorkflowStepStatus.Approved;
        DecidedBy = decidedBy;
        DecisionNotes = notes;
        DecidedAtUtc = nowUtc;
    }

    /// <summary>Records a human rejection decision.</summary>
    public void Reject(UserId decidedBy, string? notes, UtcTimestamp nowUtc)
    {
        Status = WorkflowStepStatus.Rejected;
        DecidedBy = decidedBy;
        DecisionNotes = notes;
        DecidedAtUtc = nowUtc;
    }

    /// <summary>Records an automated decision outcome, resolved by a domain policy/rule without human input.</summary>
    public void RecordAutomatedDecision(bool approved, string? notes, UtcTimestamp nowUtc)
    {
        Status = approved ? WorkflowStepStatus.AutomatedDecisionMade : WorkflowStepStatus.Rejected;
        DecisionNotes = notes;
        DecidedAtUtc = nowUtc;
    }

    /// <summary>Attaches a predictive recommendation surfaced for this step.</summary>
    public void SurfaceRecommendation(string recommendationReference, UtcTimestamp nowUtc)
    {
        RecommendationReference = recommendationReference;
        Status = WorkflowStepStatus.RecommendationSurfaced;
        DecidedAtUtc = nowUtc;
    }

    /// <summary>Marks this step as completed without a human/automated decision (e.g. a Notification step).</summary>
    public void Complete(UtcTimestamp nowUtc)
    {
        Status = WorkflowStepStatus.Completed;
        DecidedAtUtc = nowUtc;
    }

    /// <summary>Marks this step as timed out.</summary>
    public void MarkTimedOut() => Status = WorkflowStepStatus.TimedOut;

    /// <summary>Marks this step as escalated.</summary>
    public void MarkEscalated(AssignedActor escalationActor)
    {
        Status = WorkflowStepStatus.Escalated;
        AssignedActor = escalationActor;
    }

    /// <summary>Schedules a bounded retry attempt.</summary>
    public void ScheduleRetry(UtcTimestamp nextAttemptAtUtc)
    {
        AttemptCount++;
        NextAttemptAtUtc = nextAttemptAtUtc;
        Status = WorkflowStepStatus.Retrying;
    }

    /// <summary>Marks this step as skipped because its entry guard condition was not satisfied.</summary>
    public void Skip() => Status = WorkflowStepStatus.Skipped;

    /// <summary>Marks this step as cancelled.</summary>
    public void Cancel() => Status = WorkflowStepStatus.Cancelled;
}
