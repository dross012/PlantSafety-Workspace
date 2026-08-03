// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.EventMesh
//  Layer:          Api
//  File:           EnterpriseEventMeshV1Controller.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.2 Cross-System Event Propagation, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Inbound adapter for the Enterprise Event Mesh's cross-SYSTEM boundary: accepts an
//                  EnterpriseEventEnvelope POSTed by an external ERP/MES/CMMS system and dispatches
//                  IngestEnterpriseInboundEventCommand via MediatR ISender -- routing all reads/
//                  writes through the Application layer, no domain logic or data access here (same
//                  convention as every other controller in this codebase). Machine-to-machine auth:
//                  [ServiceFilter(typeof(ApiKeyAuthFilter))], mirroring the existing
//                  SecretRotationWebhookV1Controller precedent for external-system callers that
//                  cannot present an interactive JWT bearer token. Additive-only evolution.
// ============================================================================
using PlantSafety.Api.Filters;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Enterprise.EventMesh.Commands;
using PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

namespace PlantSafety.Api.Enterprise.EventMesh.Controllers.V1;

/// <summary>Inbound adapter for external ERP/MES/CMMS systems on the Enterprise Event Mesh.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/enterprise/event-mesh")]
[AllowAnonymous]
[ServiceFilter(typeof(ApiKeyAuthFilter))]
[Tags("Enterprise.EventMesh")]
public sealed class EnterpriseEventMeshV1Controller(ISender sender) : ControllerBase
{
    /// <summary>
    /// Accepts one inbound <see cref="EnterpriseEventEnvelope"/> from an external system (ERP, MES,
    /// or CMMS) and translates/dispatches it into the platform's real internal commands.
    /// </summary>
    [HttpPost("inbound")]
    [ProducesResponseType(typeof(EnterpriseInboundIngestResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> IngestInboundAsync([FromBody] EnterpriseEventEnvelope envelope, CancellationToken cancellationToken)
    {
        var command = new IngestEnterpriseInboundEventCommand(
            CommandId: Guid.NewGuid(),
            CorrelationId: envelope.CorrelationId,
            CausationId: envelope.CausationId ?? envelope.EventId,
            TenantId: envelope.TenantId,
            Envelope: envelope);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error.ErrorCode, message = result.Error.Message, correlationId = result.Error.CorrelationId });
    }
}
