// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Application
//  File:           CreateManagementOfChangeCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to submit a new ManagementOfChange (MOC) request. Carries the full
//                  command envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc,
//                  ApiVersionToken) per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.ManagementOfChange.Commands;

/// <summary>Submits a new ManagementOfChange request.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="ChangeTitle">Human-readable title of the proposed change.</param>
/// <param name="ChangeType">Change classification (Process, Equipment, Personnel, Procedure, Software).</param>
/// <param name="Justification">Business/safety justification for the change.</param>
/// <param name="RiskLevel">Risk level of the change (Low, Medium, High, Critical).</param>
/// <param name="RequesterUserId">Identity of the user requesting the change.</param>
public sealed record CreateManagementOfChangeCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string ChangeTitle,
    string ChangeType,
    string Justification,
    string RiskLevel,
    Guid RequesterUserId) : ICommand<Result<CreateManagementOfChangeResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateManagementOfChangeResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateManagementOfChangeCommand"/> from raw, untrusted transport values.</summary>
    public static CreateManagementOfChangeCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string changeTitle,
        string changeType,
        string justification,
        string riskLevel,
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
            ChangeTitle: (changeTitle ?? string.Empty).Trim(),
            ChangeType: (changeType ?? "Process").Trim(),
            Justification: (justification ?? string.Empty).Trim(),
            RiskLevel: (riskLevel ?? "Low").Trim(),
            RequesterUserId: requesterUserId);
}
