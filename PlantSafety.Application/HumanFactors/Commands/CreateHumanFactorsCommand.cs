// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Application
//  File:           CreateHumanFactorsCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to submit a new HumanFactors record under the HumanFactors bounded
//                  context. Carries the full command envelope per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HumanFactors.Commands;

/// <summary>Submits a new HumanFactors record.</summary>
public sealed record CreateHumanFactorsCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string FactorType,
    Guid? AffectedUserId,
    int SeverityRating,
    string Description,
    Guid RequesterUserId) : ICommand<Result<CreateHumanFactorsResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateHumanFactorsResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateHumanFactorsCommand"/> from raw, untrusted transport values.</summary>
    public static CreateHumanFactorsCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string factorType,
        Guid? affectedUserId,
        int severityRating,
        string description,
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
            FactorType: (factorType ?? string.Empty).Trim(),
            AffectedUserId: affectedUserId,
            SeverityRating: severityRating,
            Description: (description ?? string.Empty).Trim(),
            RequesterUserId: requesterUserId);
}
