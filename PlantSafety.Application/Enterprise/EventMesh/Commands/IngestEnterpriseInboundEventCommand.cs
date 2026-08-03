// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.EventMesh
//  Layer:          Application
//  File:           IngestEnterpriseInboundEventCommand.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.2 Cross-System Event Propagation, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Entry-point command for the Enterprise Event Mesh's inbound boundary
//                  (ERP/MES/CMMS -> Platform). Dispatched by
//                  PlantSafety.Api.Enterprise.EventMesh.Controllers.V1.EnterpriseEventMeshV1Controller
//                  after upgrading the received envelope to its latest schema version (see
//                  EventUpgradePipeline). The handler resolves the matching
//                  IEnterpriseInboundEventTranslator (one per external EventType) and lets it
//                  dispatch the target module's own public Application-layer command via ISender --
//                  never reaching into another module's Domain internals directly (Handbook Part IV
//                  §4.2.2/Part IX). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

namespace PlantSafety.Application.Enterprise.EventMesh.Commands;

/// <summary>Ingests one inbound <see cref="EnterpriseEventEnvelope"/> from an external system.</summary>
public sealed record IngestEnterpriseInboundEventCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    EnterpriseEventEnvelope Envelope) : ICommand<Result<EnterpriseInboundIngestResult>>
{
    /// <inheritdoc/>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <inheritdoc/>
    public string ApiVersionToken { get; init; } = "v1";

    /// <inheritdoc/>
    public Guid ActorId => Envelope.ActorId ?? Guid.Empty;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<EnterpriseInboundIngestResult>>.CausationId => CausationId;
}
