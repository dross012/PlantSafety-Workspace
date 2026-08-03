// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           AddWorkflowStepDefinitionCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to add a step to a draft (not-yet-activated) WorkflowDefinition (Phase
//                  5.1: "Each definition needs: steps ... assigned roles/actors, conditions").
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Workflow.Commands;

/// <summary>Adds a step to a draft WorkflowDefinition's step graph.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="WorkflowDefinitionId">Identity of the target WorkflowDefinition.</param>
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
/// <param name="RequestedByUserId">Identity of the user authoring the step.</param>
public sealed record AddWorkflowStepDefinitionCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid WorkflowDefinitionId,
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
    int RetryBaseDelaySeconds,
    Guid RequestedByUserId) : ICommand<Result<AddWorkflowStepDefinitionResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<AddWorkflowStepDefinitionResult>>.CausationId => CausationId;
}

/// <summary>Result of successfully adding a step to a WorkflowDefinition.</summary>
/// <param name="WorkflowDefinitionId">Identity of the affected definition.</param>
/// <param name="StepId">Identity of the newly added step.</param>
public sealed record AddWorkflowStepDefinitionResult(Guid WorkflowDefinitionId, Guid StepId);
