// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Contracts.Enterprise.EventMesh
//  Layer:          SharedKernel
//  File:           IExternalSystemPublisher.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3 Enterprise Event Mesh, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Outbound publishing abstraction for the Enterprise Event Mesh: platform-originated
//                  domain events, mapped to an <see cref="EnterpriseEventEnvelope"/>, are "sent" to a
//                  single external system (ERP, MES, or CMMS) through an implementation of this
//                  interface. Real implementations (e.g. an HTTP-based one) MUST gracefully no-op
//                  (log and report a non-error "skipped" outcome) when their target endpoint is not
//                  configured, matching this codebase's existing "optional, gracefully degrading"
//                  integration convention (e.g. BlobStorageOptions defaulting to the Azurite
//                  emulator when unset) -- no live ERP/MES/CMMS credentials exist in this
//                  environment. Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.Results;

namespace PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

/// <summary>
/// Publishes an <see cref="EnterpriseEventEnvelope"/> to a single external system on the Enterprise
/// Event Mesh's outbound boundary.
/// </summary>
public interface IExternalSystemPublisher
{
    /// <summary>The external system this publisher instance targets.</summary>
    EnterpriseSourceSystem TargetSystem { get; }

    /// <summary>
    /// Attempts to publish <paramref name="envelope"/> to <see cref="TargetSystem"/>.
    /// </summary>
    /// <returns>
    /// A successful <see cref="Result{T}"/> whose value is <see langword="true"/> if the envelope was
    /// actually transmitted, or <see langword="false"/> if the publish was gracefully skipped because
    /// <see cref="TargetSystem"/>'s endpoint is not currently configured. Only genuine transmission
    /// failures (e.g. a configured endpoint that rejected the request) produce a failed
    /// <see cref="Result{T}"/>.
    /// </returns>
    Task<Result<bool>> PublishAsync(EnterpriseEventEnvelope envelope, CancellationToken cancellationToken);
}
