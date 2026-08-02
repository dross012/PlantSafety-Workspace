// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Application
//  File:           CreateDigitalTwinCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to register a new DigitalTwin model registry record -- distinct from
//                  the pre-existing "Twin" Features vertical. Carries the full command envelope
//                  per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.DigitalTwin.Commands;

/// <summary>Registers a new DigitalTwin model registry record.</summary>
public sealed record CreateDigitalTwinCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string TwinName,
    Guid? AssetReferenceId,
    string ModelType,
    Guid RequesterUserId) : ICommand<Result<CreateDigitalTwinResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateDigitalTwinResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateDigitalTwinCommand"/> from raw, untrusted transport values.</summary>
    public static CreateDigitalTwinCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string twinName,
        Guid? assetReferenceId,
        string modelType,
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
            TwinName: (twinName ?? string.Empty).Trim(),
            AssetReferenceId: assetReferenceId,
            ModelType: (modelType ?? string.Empty).Trim(),
            RequesterUserId: requesterUserId);
}
