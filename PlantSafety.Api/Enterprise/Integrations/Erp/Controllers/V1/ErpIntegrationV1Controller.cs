// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Enterprise.Integrations.Erp
//  Layer:          Api
//  File:           ErpIntegrationV1Controller.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §1.1 Enterprise System Integrations, ERP)
//  SchemaVersion:  2026.1
//  Description:    On-demand ERP integration operations: asset-master-data outbound sync (no
//                  event-driven trigger exists -- CrossDomain.Machines emits no domain events, see
//                  SyncMachineAssetToErpCommand's own header) and Tenant/Site cost-center-mapping
//                  reference-data management. Routes all reads/writes through MediatR ISender to the
//                  Application layer; no domain logic or data access here. Additive-only evolution.
// ============================================================================
using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.Enterprise.Integrations.Erp.Authorization;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Enterprise.Integrations.Erp.Commands;
using PlantSafety.Application.Enterprise.Integrations.Erp.CostCenters;

namespace PlantSafety.Api.Enterprise.Integrations.Erp.Controllers.V1;

/// <summary>V1 controller for on-demand ERP integration operations.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/enterprise/integrations/erp")]
[Authorize(Policy = ErpIntegrationAuthorizationPolicies.Approve)]
[Tags("Enterprise.Integrations.Erp")]
public sealed class ErpIntegrationV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory) : ControllerBase
{
    /// <summary>Synchronizes this tenant's (optionally site-scoped) machine/asset records to the external ERP system's asset master.</summary>
    [HttpPost("assets/sync")]
    [ProducesResponseType(typeof(ApiResponse<SyncMachineAssetToErpResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncMachineAssetsAsync([FromQuery] Guid? siteId, CancellationToken cancellationToken)
    {
        var env = envelopeFactory.NewCommandEnvelope();

        var command = SyncMachineAssetToErpCommand.FromDto(
            tenantId: env.TenantId,
            siteId: siteId,
            requestedByUserId: env.ActorId,
            correlationId: env.CorrelationId,
            causationId: env.CausationId);

        var result = await sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves the effective cost center mapping for this tenant (optionally site-scoped).</summary>
    [HttpGet("cost-centers")]
    [ProducesResponseType(typeof(ApiResponse<CostCenterMapping>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCostCenterMappingAsync([FromQuery] Guid? siteId, CancellationToken cancellationToken)
    {
        var env = envelopeFactory.NewQueryEnvelope();
        var result = await sender.Send(new GetCostCenterMappingQuery(env.TenantId, siteId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Creates or replaces the cost center mapping for this tenant (optionally site-scoped).</summary>
    [HttpPut("cost-centers")]
    [ProducesResponseType(typeof(ApiResponse<CostCenterMapping>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetCostCenterMappingAsync([FromBody] SetCostCenterMappingRequest request, CancellationToken cancellationToken)
    {
        var env = envelopeFactory.NewCommandEnvelope();

        var command = new SetCostCenterMappingCommand(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            ExternalCostCenterCode: request.ExternalCostCenterCode,
            Description: request.Description,
            UpdatedByUserId: env.ActorId);

        var result = await sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
}

/// <summary>Request body for <see cref="ErpIntegrationV1Controller.SetCostCenterMappingAsync"/>.</summary>
/// <param name="SiteId">Site identifier, or null for a tenant-wide default mapping.</param>
/// <param name="ExternalCostCenterCode">The external ERP system's cost center code.</param>
/// <param name="Description">Human-readable description of the cost center.</param>
public sealed record SetCostCenterMappingRequest(Guid? SiteId, string ExternalCostCenterCode, string? Description);
