// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Application
//  File:           CreateOperationalSafetyCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to submit a new OperationalSafety record under the OperationalSafety
//                  bounded context. Carries the full command envelope per Handbook Rule 2.1.
//                  Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.OperationalSafety.Commands;

/// <summary>Submits a new OperationalSafety record.</summary>
public sealed record CreateOperationalSafetyCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string SafetyDomain,
    string RiskLevel,
    Guid OwnerUserId,
    DateTime? ReviewDueAtUtc,
    Guid RequesterUserId) : ICommand<Result<CreateOperationalSafetyResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateOperationalSafetyResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateOperationalSafetyCommand"/> from raw, untrusted transport values.</summary>
    public static CreateOperationalSafetyCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string safetyDomain,
        string riskLevel,
        Guid ownerUserId,
        DateTime? reviewDueAtUtc,
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
            SafetyDomain: (safetyDomain ?? string.Empty).Trim(),
            RiskLevel: (riskLevel ?? "Low").Trim(),
            OwnerUserId: ownerUserId,
            ReviewDueAtUtc: reviewDueAtUtc,
            RequesterUserId: requesterUserId);
}
