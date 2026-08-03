// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           MakeAutomatedDecisionCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command recording an automated-decision outcome for a WorkflowInstance's
//                  current step, resolved by a caller-supplied domain policy/rule without human
//                  input (Phase 5.3 human + machine collaboration: "Automated decision steps").
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Workflow.Commands;

/// <summary>Records an automated-decision outcome on a WorkflowInstance's current step.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="WorkflowInstanceId">Identity of the instance whose current step the decision applies to.</param>
/// <param name="Approved">The resolved automated outcome.</param>
/// <param name="Notes">Explanatory notes recorded alongside the automated outcome.</param>
/// <param name="RequestedByUserId">Identity of the system process (or the user triggering the automated evaluation) recording the decision.</param>
public sealed record MakeAutomatedDecisionCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid WorkflowInstanceId,
    bool Approved,
    string? Notes,
    Guid RequestedByUserId) : ICommand<Result<WorkflowInstanceStatusResult>>, IWorkflowInstanceScopedCommand
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<WorkflowInstanceStatusResult>>.CausationId => CausationId;
}
