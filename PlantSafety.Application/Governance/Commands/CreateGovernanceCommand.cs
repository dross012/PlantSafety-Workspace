// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Application
//  File:           CreateGovernanceCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to submit a new Governance record under the Governance bounded
//                  context. Carries the full command envelope per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Governance.Commands;

/// <summary>Submits a new Governance record.</summary>
public sealed record CreateGovernanceCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string GovernanceArea,
    Guid? PolicyReferenceId,
    int ReviewCycleMonths,
    Guid OwnerUserId,
    Guid RequesterUserId) : ICommand<Result<CreateGovernanceResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateGovernanceResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateGovernanceCommand"/> from raw, untrusted transport values.</summary>
    public static CreateGovernanceCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string governanceArea,
        Guid? policyReferenceId,
        int reviewCycleMonths,
        Guid ownerUserId,
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
            GovernanceArea: (governanceArea ?? string.Empty).Trim(),
            PolicyReferenceId: policyReferenceId,
            ReviewCycleMonths: reviewCycleMonths <= 0 ? 12 : reviewCycleMonths,
            OwnerUserId: ownerUserId,
            RequesterUserId: requesterUserId);
}
