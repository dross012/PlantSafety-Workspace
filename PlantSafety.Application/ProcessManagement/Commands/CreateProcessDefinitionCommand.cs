// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Application
//  File:           CreateProcessDefinitionCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to submit a new ProcessDefinition record under the ProcessManagement
//                  bounded context. Carries the full command envelope per Handbook Rule 2.1.
//                  Mirrors PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ProcessManagement.Commands;

/// <summary>Submits a new ProcessDefinition record.</summary>
public sealed record CreateProcessDefinitionCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string ProcessName,
    string Category,
    Guid OwnerId,
    Guid RequesterUserId) : ICommand<Result<CreateProcessDefinitionResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateProcessDefinitionResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateProcessDefinitionCommand"/> from raw, untrusted transport values.</summary>
    public static CreateProcessDefinitionCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string processName,
        string category,
        Guid ownerId,
        Guid requesterUserId,
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
            ProcessName: (processName ?? string.Empty).Trim(),
            Category: (category ?? string.Empty).Trim(),
            OwnerId: ownerId,
            RequesterUserId: requesterUserId);
}
