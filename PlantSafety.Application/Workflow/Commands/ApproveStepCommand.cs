// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           ApproveStepCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command carrying a human actor's approval decision on the currently active
//                  HumanApproval step of a WorkflowInstance (Phase 5.3 human + machine
//                  collaboration: "Human approval steps").
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Workflow.Commands;

/// <summary>Records a human approval decision on a WorkflowInstance's current step.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="WorkflowInstanceId">Identity of the instance whose current step is being approved.</param>
/// <param name="Notes">Optional free-text notes captured with the approval.</param>
/// <param name="ApprovedByUserId">Identity of the human actor recording the approval.</param>
public sealed record ApproveStepCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid WorkflowInstanceId,
    string? Notes,
    Guid ApprovedByUserId) : ICommand<Result<WorkflowInstanceStatusResult>>, IWorkflowInstanceScopedCommand
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => ApprovedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<WorkflowInstanceStatusResult>>.CausationId => CausationId;
}
