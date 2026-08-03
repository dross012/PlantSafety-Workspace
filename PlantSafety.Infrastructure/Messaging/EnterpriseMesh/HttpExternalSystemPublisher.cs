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
// ============================================================================
using System.Net.Http.Json;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Results;

namespace PlantSafety.Infrastructure.Messaging.EnterpriseMesh;

/// <summary>HTTP-based <see cref="IExternalSystemPublisher"/> for one external system's Enterprise Event Mesh endpoint.</summary>
public sealed class HttpExternalSystemPublisher(
    EnterpriseSourceSystem targetSystem,
    string endpointUrl,
    HttpClient httpClient,
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
