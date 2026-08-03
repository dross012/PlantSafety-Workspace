// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           RejectStepCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command carrying a human actor's rejection decision on the currently active
//                  HumanApproval step of a WorkflowInstance. Terminal: the instance transitions to
//                  Rejected (Phase 5.3 human + machine collaboration).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Workflow.Commands;

/// <summary>Records a human rejection decision on a WorkflowInstance's current step.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="WorkflowInstanceId">Identity of the instance whose current step is being rejected.</param>
/// <param name="Reason">Reason for the rejection.</param>
/// <param name="RejectedByUserId">Identity of the human actor recording the rejection.</param>
public sealed record RejectStepCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid WorkflowInstanceId,
    string? Reason,
    Guid RejectedByUserId) : ICommand<Result<WorkflowInstanceStatusResult>>, IWorkflowInstanceScopedCommand
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RejectedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<WorkflowInstanceStatusResult>>.CausationId => CausationId;
}
