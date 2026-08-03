// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           EscalateWorkflowCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to manually escalate a WorkflowInstance's current step to its
//                  configured (or fallback) escalation actor (Phase 5.2 timeout/escalation rules).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Workflow.Commands;

/// <summary>Escalates a WorkflowInstance's current step to its designated escalation actor.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="WorkflowInstanceId">Identity of the instance to escalate.</param>
/// <param name="RequestedByUserId">Identity of the user or system process triggering the escalation.</param>
public sealed record EscalateWorkflowCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid WorkflowInstanceId,
    Guid RequestedByUserId) : ICommand<Result<WorkflowInstanceStatusResult>>, IWorkflowInstanceScopedCommand
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<WorkflowInstanceStatusResult>>.CausationId => CausationId;
}
