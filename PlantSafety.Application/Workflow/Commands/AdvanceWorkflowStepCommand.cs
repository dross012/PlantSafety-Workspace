// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           AdvanceWorkflowStepCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to advance a WorkflowInstance from its current (already-resolved) step
//                  to the next step in its WorkflowDefinition's guarded transition graph, or to
//                  Completed when no further step follows (Phase 5.2 workflow runtime).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Workflow.Commands;

/// <summary>Advances a WorkflowInstance to the next step (or to Completed) per its definition's guarded transition graph.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="WorkflowInstanceId">Identity of the instance to advance.</param>
/// <param name="ContextUpdates">Additional context data key/value pairs to merge in before resolving the next transition.</param>
/// <param name="RequestedByUserId">Identity of the user or system process triggering the advance.</param>
public sealed record AdvanceWorkflowStepCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid WorkflowInstanceId,
    IReadOnlyDictionary<string, string>? ContextUpdates,
    Guid RequestedByUserId) : ICommand<Result<WorkflowInstanceStatusResult>>, IWorkflowInstanceScopedCommand
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<WorkflowInstanceStatusResult>>.CausationId => CausationId;
}
