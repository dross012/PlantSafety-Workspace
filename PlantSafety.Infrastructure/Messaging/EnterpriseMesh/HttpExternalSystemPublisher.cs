// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Messaging.EnterpriseMesh
//  Layer:          Infrastructure
//  File:           HttpExternalSystemPublisher.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.2 Cross-System Event Propagation, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Real, working HTTP-based IExternalSystemPublisher: POSTs the
//                  EnterpriseEventEnvelope as JSON to a configurable endpoint. Gracefully no-ops
//                  (logs at Debug, returns a successful "skipped" Result) when its configured
//                  endpoint is empty -- matching this codebase's existing "optional, gracefully
//                  degrading" integration convention (see BlobStorageOptions/AzureBlobStorageService
//                  defaulting to the Azurite emulator connection string when unset). No live
//                  ERP/MES credentials exist in this sandboxed environment; integration tests
//                  exercise this class against an in-memory/fake HTTP handler (see
//                  Shared.EventBus.Tests-style precedent) rather than a live endpoint. Additive-only
//                  evolution.
//
//                  2026-08-03 UPDATE (Phase 7 §5.3 Data Residency wiring): publishing an envelope to
//                  an external ERP/MES/CMMS system is a genuinely cross-region-relevant operation --
//                  the external endpoint may sit outside the tenant's required residency zone. Before
//                  any HTTP transmission this now calls IDataResidencyEnforcer.IsSatisfied(tenantId)
//                  (reusing the same tenant-vs-current-deployment-region comparison the enforcer
//                  already performs for tenant data exports) and rejects the publish -- loudly logged,
//                  never silently dropped -- when the tenant's policy does not permit it. Additive-only
//                  evolution.
// ============================================================================
using System.Net.Http.Json;
using PlantSafety.Application.Enterprise.MultiRegion.DataResidency;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Results;

namespace PlantSafety.Infrastructure.Messaging.EnterpriseMesh;

/// <summary>HTTP-based <see cref="IExternalSystemPublisher"/> for one external system's Enterprise Event Mesh endpoint.</summary>
public sealed class HttpExternalSystemPublisher(
    EnterpriseSourceSystem targetSystem,
    string endpointUrl,
    HttpClient httpClient,
    IDataResidencyEnforcer dataResidencyEnforcer,
    ILogger<HttpExternalSystemPublisher> logger) : IExternalSystemPublisher
{
    /// <inheritdoc/>
    public EnterpriseSourceSystem TargetSystem { get; } = targetSystem;

    /// <inheritdoc/>
    public async Task<Result<bool>> PublishAsync(EnterpriseEventEnvelope envelope, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(endpointUrl))
        {
            logger.LogDebug(
                "EnterpriseEventMesh: {TargetSystem} endpoint is not configured; skipping outbound publish of {EventType} (EventId={EventId}).",
                TargetSystem, envelope.EventType, envelope.EventId);
            return Result<bool>.Success(false);
        }

        if (envelope.TenantId != Guid.Empty && !dataResidencyEnforcer.IsSatisfied(envelope.TenantId))
        {
            logger.LogWarning(
                "EnterpriseEventMesh: rejected outbound publish of {EventType} (EventId={EventId}) for Tenant {TenantId} to " +
                "{TargetSystem} -- the tenant's configured data-residency policy does not permit sending data outside the " +
                "platform's current deployment region.",
                envelope.EventType, envelope.EventId, envelope.TenantId, TargetSystem);

            return Result<bool>.Failure(new Error(
                ErrorCode.Create("enterprise.eventmesh.residencyviolation"),
                ErrorType.Forbidden,
                $"Tenant {envelope.TenantId}'s data-residency policy does not permit publishing to {TargetSystem} from the platform's current deployment region."));
        }

        try
        {
            using var response = await httpClient.PostAsJsonAsync(endpointUrl, envelope, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "EnterpriseEventMesh: {TargetSystem} rejected {EventType} (EventId={EventId}) with status {StatusCode}.",
                    TargetSystem, envelope.EventType, envelope.EventId, (int)response.StatusCode);

                return Result<bool>.Failure(new Error(
                    ErrorCode.Create("enterprise.eventmesh.publishrejected"),
                    ErrorType.DependencyFailure,
                    $"{TargetSystem} endpoint rejected the event with status {(int)response.StatusCode}."));
            }

            logger.LogInformation(
                "EnterpriseEventMesh: published {EventType} (EventId={EventId}) to {TargetSystem}.",
                envelope.EventType, envelope.EventId, TargetSystem);
            return Result<bool>.Success(true);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex,
                "EnterpriseEventMesh: transport failure publishing {EventType} (EventId={EventId}) to {TargetSystem}.",
                envelope.EventType, envelope.EventId, TargetSystem);

            return Result<bool>.Failure(new Error(
                ErrorCode.Create("enterprise.eventmesh.publishtransportfailure"),
                ErrorType.DependencyUnavailable,
                $"Transport failure while publishing to {TargetSystem}."));
        }
    }
}
