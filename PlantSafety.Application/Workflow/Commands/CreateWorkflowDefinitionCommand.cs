// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           CreateWorkflowDefinitionCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to create a new, inactive (draft) WorkflowDefinition (Phase 5.1
//                  workflow definitions). Carries the full command envelope (CorrelationId,
//                  CausationId, CommandId, RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Workflow.Commands;

/// <summary>Creates a new, inactive WorkflowDefinition.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="WorkflowTypeCode">Workflow category code: Approval, Escalation, Notification, or MultiStepProcess.</param>
/// <param name="Name">Human-readable workflow name.</param>
/// <param name="Description">Free-form description of the workflow's purpose.</param>
/// <param name="RequestedByUserId">Identity of the user authoring the definition.</param>
public sealed record CreateWorkflowDefinitionCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string WorkflowTypeCode,
    string Name,
    string Description,
    Guid RequestedByUserId) : ICommand<Result<CreateWorkflowDefinitionResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateWorkflowDefinitionResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateWorkflowDefinitionCommand"/> from raw, untrusted transport values.</summary>
    public static CreateWorkflowDefinitionCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string workflowTypeCode,
        string name,
        string description,
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
            WorkflowTypeCode: (workflowTypeCode ?? string.Empty).Trim(),
            Name: (name ?? string.Empty).Trim(),
            Description: (description ?? string.Empty).Trim(),
            RequestedByUserId: requestedByUserId);
}

/// <summary>Result of successfully creating a WorkflowDefinition.</summary>
/// <param name="WorkflowDefinitionId">Identity of the newly created definition.</param>
/// <param name="IsActive">Whether the definition is active (always false immediately after creation).</param>
public sealed record CreateWorkflowDefinitionResult(Guid WorkflowDefinitionId, bool IsActive);
