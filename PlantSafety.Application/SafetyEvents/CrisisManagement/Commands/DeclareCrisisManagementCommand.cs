// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Application
//  File:           DeclareCrisisManagementCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to declare a new CrisisManagement crisis under the
//                  SafetyEvents.CrisisManagement bounded context. Carries the full command
//                  envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc,
//                  ApiVersionToken) per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.CrisisManagement.Commands;

/// <summary>Declares a new CrisisManagement crisis.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="Title">Short crisis title.</param>
/// <param name="Severity">Severity classification (e.g., Low/Medium/High/Critical).</param>
/// <param name="DeclaredAtUtc">UTC instant the crisis was declared.</param>
/// <param name="DeclaredByUserId">Identity of the user or system actor declaring the crisis.</param>
public sealed record DeclareCrisisManagementCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string Title,
    string Severity,
    DateTime DeclaredAtUtc,
    Guid DeclaredByUserId) : ICommand<Result<CrisisManagementStatusResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => DeclaredByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CrisisManagementStatusResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="DeclareCrisisManagementCommand"/> from raw, untrusted transport values.</summary>
    public static DeclareCrisisManagementCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string title,
        string severity,
        DateTime declaredAtUtc,
        Guid declaredByUserId,
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
            Title: (title ?? string.Empty).Trim(),
            Severity: (severity ?? string.Empty).Trim(),
            DeclaredAtUtc: declaredAtUtc,
            DeclaredByUserId: declaredByUserId);
}
