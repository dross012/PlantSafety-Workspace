// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           StartWorkflowCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to start a new WorkflowInstance from an activated WorkflowDefinition
//                  (Phase 5.2 workflow runtime).
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Workflow.Commands;

/// <summary>Starts a new WorkflowInstance from an activated WorkflowDefinition.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="WorkflowDefinitionId">Identity of the activated WorkflowDefinition to start.</param>
/// <param name="InitialContextData">Seed context data used to evaluate guard conditions as the instance progresses.</param>
/// <param name="RequestedByUserId">Identity of the user initiating the instance.</param>
public sealed record StartWorkflowCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    Guid WorkflowDefinitionId,
    IReadOnlyDictionary<string, string>? InitialContextData,
    Guid RequestedByUserId) : ICommand<Result<WorkflowInstanceStatusResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<WorkflowInstanceStatusResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="StartWorkflowCommand"/> from raw, untrusted transport values.</summary>
    public static StartWorkflowCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        Guid workflowDefinitionId,
        IReadOnlyDictionary<string, string>? initialContextData,
        Guid requestedByUserId,
        Guid? correlationId = null,
        Guid? causationId = null,
        string apiVersionToken = "v1")
        => new(
            CommandId: Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationId: causationId ?? Guid.NewGuid(),
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: apiVersionToken,
            TenantId: tenantId,
            SiteId: siteId,
            WorkflowDefinitionId: workflowDefinitionId,
            InitialContextData: initialContextData,
            RequestedByUserId: requestedByUserId);
}
