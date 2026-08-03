// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.Integrations.Erp
//  Layer:          Application
//  File:           SyncMachineAssetToErpCommand.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.1 Enterprise System Integrations, ERP)
//  SchemaVersion:  2026.1
//  Description:    On-demand asset-master-data outbound sync: maps the real Machines read model
//                  (PlantSafety.Application.CrossDomain.Machines.Models.MachinesItemDto -- CrossDomain
//                  is a projection-only bounded context per Handbook Part XXII and owns no domain
//                  events, so there is no event-driven trigger for this sync; it is dispatched
//                  on-demand or by a scheduled job) onto the enterprise canonical asset shape via the
//                  real MachineToCanonicalAssetMapper, wraps it in an EnterpriseEventEnvelope, and
//                  publishes it to every configured ERP-targeted IExternalSystemPublisher. Additive-
//                  only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.CrossDomain.Machines.Mappings;
using PlantSafety.Application.CrossDomain.Machines.Repositories;
using PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

namespace PlantSafety.Application.Enterprise.Integrations.Erp.Commands;

/// <summary>Synchronizes one or more machine/asset records to the external ERP system's asset master.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="ApiVersionToken">API version token.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="RequestedByUserId">Identity of the user or scheduled job initiating the sync.</param>
public sealed record SyncMachineAssetToErpCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    Guid RequestedByUserId) : ICommand<Result<SyncMachineAssetToErpResult>>
{
    /// <inheritdoc/>
    public Guid ActorId => RequestedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<SyncMachineAssetToErpResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="SyncMachineAssetToErpCommand"/> from raw, untrusted transport values.</summary>
    public static SyncMachineAssetToErpCommand FromDto(
        Guid tenantId,
        Guid? siteId,
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
            RequestedByUserId: requestedByUserId);
}

/// <summary>Result of a successful <see cref="SyncMachineAssetToErpCommand"/> execution.</summary>
/// <param name="AssetsAttempted">Number of asset records mapped and attempted for publish.</param>
/// <param name="AssetsPublished">Number of asset records genuinely transmitted (endpoint configured and accepted).</param>
/// <param name="AssetsSkipped">Number of asset records gracefully skipped because no ERP endpoint is configured.</param>
public sealed record SyncMachineAssetToErpResult(int AssetsAttempted, int AssetsPublished, int AssetsSkipped);

/// <summary>Handles <see cref="SyncMachineAssetToErpCommand"/>.</summary>
public sealed class SyncMachineAssetToErpCommandHandler(
    IMachinesRepository machinesRepository,
    IEnumerable<IExternalSystemPublisher> publishers,
    ILogger<SyncMachineAssetToErpCommandHandler> logger)
    : IRequestHandler<SyncMachineAssetToErpCommand, Result<SyncMachineAssetToErpResult>>
{
    /// <summary>Canonical, namespaced EventType for asset-master sync envelopes.</summary>
    public const string AssetSyncEventType = "plantsafety.asset.mastersync.v1";

    /// <inheritdoc/>
    public async Task<Result<SyncMachineAssetToErpResult>> Handle(
        SyncMachineAssetToErpCommand request, CancellationToken cancellationToken)
    {
        var erpPublishers = publishers.Where(p => p.TargetSystem == EnterpriseSourceSystem.Erp).ToList();

        var machines = await machinesRepository.GetMachinesAsync(
            request.TenantId, request.SiteId, DateTime.MinValue, DateTime.UtcNow, cancellationToken);

        var attempted = 0;
        var published = 0;
        var skipped = 0;

        foreach (var machine in machines)
        {
            var canonical = machine.ToCanonicalV1();

            var envelope = EnterpriseEventEnvelope.Create(
                eventType: AssetSyncEventType,
                schemaVersion: CanonicalAssetV1.SchemaVersion,
                sourceSystem: EnterpriseSourceSystem.Platform,
                occurredAtUtc: DateTime.UtcNow,
                correlationId: request.CorrelationId,
                causationId: request.CausationId,
                tenantId: request.TenantId,
                actorId: request.ActorId == Guid.Empty ? null : request.ActorId,
                payload: canonical);

            attempted++;

            foreach (var publisher in erpPublishers)
            {
                var result = await publisher.PublishAsync(envelope, cancellationToken);

                if (result.IsFailure)
                {
                    logger.LogWarning(
                        "Enterprise.Integrations.Erp: asset master sync for {AssetId} to {TargetSystem} failed: {Message}",
                        canonical.AssetId, publisher.TargetSystem, result.Error!.Message);
                    continue;
                }

                if (result.Value)
                    published++;
                else
                    skipped++;
            }
        }

        return Result<SyncMachineAssetToErpResult>.Success(
            new SyncMachineAssetToErpResult(attempted, published, skipped));
    }
}
