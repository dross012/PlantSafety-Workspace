// ============================================================================
// File: WorkflowRequests.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (API Contract Validation Agent)
// Created: 2026-08-02
// Last Updated: 2026-08-02
// Description: External-facing request DTOs for the Workflow module's V1 controller. Mirror the
//              caller-supplied fields of the corresponding Workflow.Commands records field-for-field
//              (Handbook Rule 1.4, Canonical DTO Rule), and mirror
//              PlantSafety.UI.Modules.Workflow.Models.* request shapes exactly so the UI's typed
//              client can round-trip without translation.
// ============================================================================
//
// Onboarding Notes:
// - Bound from the request body ([FromBody]) by WorkflowV1Controller.
// - Validated implicitly by ASP.NET Core model binding; commands themselves enforce invariants.
//
// Rationale:
// - Decouples the external HTTP contract from internal Application-layer command types.
//
// ============================================================================

namespace PlantSafety.Api.Workflow.Contracts.Requests;

/// <summary>Request body for starting a new WorkflowInstance.</summary>
/// <param name="WorkflowDefinitionId">Identity of the activated WorkflowDefinition to start.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="InitialContextData">Seed context data used to evaluate guard conditions as the instance progresses.</param>
public sealed record StartWorkflowRequest(
    Guid WorkflowDefinitionId,
    Guid? SiteId,
    IReadOnlyDictionary<string, string>? InitialContextData);

/// <summary>Request body for recording a human approval decision on a WorkflowInstance's current step.</summary>
/// <param name="Notes">Optional free-text notes captured with the approval.</param>
public sealed record ApproveStepRequest(string? Notes);

/// <summary>Request body for recording a human rejection decision on a WorkflowInstance's current step.</summary>
/// <param name="Reason">Reason for the rejection.</param>
public sealed record RejectStepRequest(string? Reason);

/// <summary>Request body for advancing a WorkflowInstance to its next step.</summary>
/// <param name="ContextUpdates">Additional context data key/value pairs to merge in before resolving the next transition.</param>
public sealed record AdvanceWorkflowStepRequest(IReadOnlyDictionary<string, string>? ContextUpdates);

/// <summary>Request body for cancelling a WorkflowInstance before it reaches a terminal state.</summary>
/// <param name="Reason">Reason for the cancellation.</param>
public sealed record CancelWorkflowRequest(string? Reason);

/// <summary>Request body for creating a new, inactive (draft) WorkflowDefinition.</summary>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="WorkflowTypeCode">Workflow category code: Approval, Escalation, Notification, or MultiStepProcess.</param>
/// <param name="Name">Human-readable workflow name.</param>
/// <param name="Description">Free-form description of the workflow's purpose.</param>
public sealed record CreateWorkflowDefinitionRequest(
    Guid? SiteId,
    string WorkflowTypeCode,
    string Name,
    string Description);

/// <summary>Request body for adding a step to a draft WorkflowDefinition's step graph.</summary>
/// <param name="Order">1-based ordinal position of the step within the graph.</param>
/// <param name="Name">Human-readable step name.</param>
/// <param name="StepTypeCode">Step resolution mode code: HumanApproval, AutomatedDecision, PredictiveRecommendation, Notification, or Escalation.</param>
/// <param name="AssignedRoleCode">Role code assigned to resolve this step, or null when <paramref name="AssignedUserId"/> is supplied.</param>
/// <param name="AssignedUserId">Specific user assigned to resolve this step, or null when <paramref name="AssignedRoleCode"/> is supplied.</param>
/// <param name="EscalationRoleCode">Role code escalated to when this step times out with Action=Escalate.</param>
/// <param name="EntryConditionField">Context data field name the entry guard condition evaluates, or null/empty for an unconditional step.</param>
/// <param name="EntryConditionOperatorCode">Guard operator code: Equals, NotEquals, GreaterThan, LessThan, or Contains.</param>
/// <param name="EntryConditionExpectedValue">Expected value the entry guard condition compares against.</param>
/// <param name="TimeoutSeconds">Timeout window in seconds, or null when the step never times out.</param>
/// <param name="TimeoutActionCode">Action taken on timeout: Escalate, Fail, or Retry. Ignored when <paramref name="TimeoutSeconds"/> is null.</param>
/// <param name="RetryMaxAttempts">Maximum bounded retry attempts, or null when the step is never retried.</param>
/// <param name="RetryBackoffStrategyCode">Deterministic backoff strategy code: None, Fixed, Linear, or Exponential.</param>
/// <param name="RetryBaseDelaySeconds">Base delay in seconds used as the seed for the backoff computation.</param>
public sealed record AddWorkflowStepDefinitionRequest(
    int Order,
    string Name,
    string StepTypeCode,
    string? AssignedRoleCode,
    Guid? AssignedUserId,
    string? EscalationRoleCode,
    string? EntryConditionField,
    string EntryConditionOperatorCode,
    string? EntryConditionExpectedValue,
    int? TimeoutSeconds,
    string TimeoutActionCode,
    int? RetryMaxAttempts,
    string RetryBackoffStrategyCode,
    int RetryBaseDelaySeconds);

/// <summary>Request body for adding a directed transition between two steps of a draft WorkflowDefinition.</summary>
/// <param name="FromStepId">Source step of the transition.</param>
/// <param name="ToStepId">Destination step of the transition.</param>
/// <param name="GuardField">Context data field name the guard condition evaluates, or null/empty for an unconditional transition.</param>
/// <param name="GuardOperatorCode">Guard operator code: Equals, NotEquals, GreaterThan, LessThan, or Contains.</param>
/// <param name="GuardExpectedValue">Expected value the guard condition compares against.</param>
/// <param name="IsDefault">Whether this transition is the fallback taken when no guarded transition out of <paramref name="FromStepId"/> is satisfied.</param>
public sealed record AddWorkflowTransitionRequest(
    Guid FromStepId,
    Guid ToStepId,
    string? GuardField,
    string GuardOperatorCode,
    string? GuardExpectedValue,
    bool IsDefault);

/// <summary>Request body for recording an automated-decision outcome on a WorkflowInstance's current step.</summary>
/// <param name="Approved">The resolved automated outcome.</param>
/// <param name="Notes">Explanatory notes recorded alongside the automated outcome.</param>
public sealed record MakeAutomatedDecisionRequest(bool Approved, string? Notes);

/// <summary>Request body for attaching a predictive recommendation to a WorkflowInstance's current step.</summary>
/// <param name="RecommendationReference">
/// Opaque reference to the source recommendation (e.g. an Intelligence.PredictiveSafety alert id
/// or Analytics.PredictiveAnalytics forecast id), carried as a string to avoid a compile-time
/// dependency on either module's Domain layer.
/// </param>
public sealed record SurfaceRecommendationRequest(string RecommendationReference);
