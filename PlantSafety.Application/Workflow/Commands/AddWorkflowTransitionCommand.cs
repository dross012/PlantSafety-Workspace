// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           AddWorkflowTransitionCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to add a directed, optionally-guarded transition between two steps of a
//                  draft WorkflowDefinition (Phase 5.1: "transitions").
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Workflow.Commands;

/// <summary>Adds a directed transition between two steps of a draft WorkflowDefinition.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="WorkflowDefinitionId">Identity of the target WorkflowDefinition.</param>
/// <param name="FromStepId">Source step of the transition.</param>
/// <param name="ToStepId">Destination step of the transition.</param>
/// <param name="GuardField">Context data field name the guard condition evaluates, or null/empty for an unconditional transition.</param>
/// <param name="GuardOperatorCode">Guard operator code: Equals, NotEquals, GreaterThan, LessThan, or Contains.</param>
/// <param name="GuardExpectedValue">Expected value the guard condition compares against.</param>
/// <param name="IsDefault">Whether this transition is the fallback taken when no guarded transition out of <paramref name="FromStepId"/> is satisfied.</param>
/// <param name="RequestedByUserId">Identity of the user authoring the transition.</param>
public sealed record AddWorkflowTransitionCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid WorkflowDefinitionId,
    Guid FromStepId,
    Guid ToStepId,
    string? GuardField,
    string GuardOperatorCode,
    string? GuardExpectedValue,
    bool IsDefault,
    Guid RequestedByUserId) : ICommand<Result<AddWorkflowTransitionResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<AddWorkflowTransitionResult>>.CausationId => CausationId;
}

/// <summary>Result of successfully adding a transition to a WorkflowDefinition.</summary>
/// <param name="WorkflowDefinitionId">Identity of the affected definition.</param>
/// <param name="FromStepId">Source step of the added transition.</param>
/// <param name="ToStepId">Destination step of the added transition.</param>
public sealed record AddWorkflowTransitionResult(Guid WorkflowDefinitionId, Guid FromStepId, Guid ToStepId);
