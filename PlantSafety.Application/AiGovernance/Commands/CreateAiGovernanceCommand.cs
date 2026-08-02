// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Application
//  File:           CreateAiGovernanceCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to register a new AiGovernance record -- an AI system governance/
//                  oversight registry entry, distinct from the pre-existing "GovernedModel"
//                  Features vertical. Carries the full command envelope per Handbook Rule 2.1.
//                  Mirrors PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AiGovernance.Commands;

/// <summary>Registers a new AiGovernance record.</summary>
public sealed record CreateAiGovernanceCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string SystemName,
    string AiUseCase,
    string RiskClassification,
    Guid OwnerUserId,
    bool RequiresHumanOversight,
    Guid RequesterUserId) : ICommand<Result<CreateAiGovernanceResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateAiGovernanceResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateAiGovernanceCommand"/> from raw, untrusted transport values.</summary>
    public static CreateAiGovernanceCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string systemName,
        string aiUseCase,
        string riskClassification,
        Guid ownerUserId,
        bool requiresHumanOversight,
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
            SystemName: (systemName ?? string.Empty).Trim(),
            AiUseCase: (aiUseCase ?? string.Empty).Trim(),
            RiskClassification: (riskClassification ?? "Low").Trim(),
            OwnerUserId: ownerUserId,
            RequiresHumanOversight: requiresHumanOversight,
            RequesterUserId: requesterUserId);
}
