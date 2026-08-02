// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Application
//  File:           CreateSustainabilityCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to submit a new Sustainability initiative record. Carries the full
//                  command envelope per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Sustainability.Commands;

/// <summary>Submits a new Sustainability initiative record.</summary>
public sealed record CreateSustainabilityCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string InitiativeName,
    string Category,
    decimal? TargetValue,
    Guid OwnerUserId,
    Guid RequesterUserId) : ICommand<Result<CreateSustainabilityResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateSustainabilityResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateSustainabilityCommand"/> from raw, untrusted transport values.</summary>
    public static CreateSustainabilityCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string initiativeName,
        string category,
        decimal? targetValue,
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
            InitiativeName: (initiativeName ?? string.Empty).Trim(),
            Category: (category ?? string.Empty).Trim(),
            TargetValue: targetValue,
            OwnerUserId: ownerUserId,
            RequesterUserId: requesterUserId);
}
