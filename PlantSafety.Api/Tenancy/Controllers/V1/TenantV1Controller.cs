// ============================================================================
// File: TenantV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
// Created: 2026-08-03
// Description: V1 controller exposing the Tenancy.Tenant bounded context: provision/get/list plus
//              lifecycle management (suspend/reactivate/delete/archive/export). Every action is
//              gated at TenantAuthorizationPolicies.Manage (PlatformAdmin-and-above) -- see that
//              policy's file header for why even read access is platform-admin-only here.
// ============================================================================
//
// Onboarding Notes:
// - Routes:
//   POST   api/v1/tenancy/tenants                          -- provision a new tenant
//   GET    api/v1/tenancy/tenants/{tenantId}                -- get a single tenant
//   GET    api/v1/tenancy/tenants                           -- list every tenant
//   POST   api/v1/tenancy/tenants/{tenantId}/suspend        -- suspend a tenant
//   POST   api/v1/tenancy/tenants/{tenantId}/reactivate     -- reactivate a suspended tenant
//   DELETE api/v1/tenancy/tenants/{tenantId}                -- soft-delete a tenant
//   POST   api/v1/tenancy/tenants/{tenantId}/archive        -- archive a tenant's data
//   POST   api/v1/tenancy/tenants/{tenantId}/export         -- export a tenant's own canonical record
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Common;
using PlantSafety.Api.Tenancy.Authorization;
using PlantSafety.Api.Tenancy.Contracts.Requests;
using PlantSafety.Application.Tenancy.Tenant.Commands;
using PlantSafety.Application.Tenancy.Tenant.Models;
using PlantSafety.Application.Tenancy.Tenant.Queries;

namespace PlantSafety.Api.Tenancy.Controllers.V1;

/// <summary>V1 controller for the Tenancy.Tenant bounded context.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tenancy/tenants")]
[Authorize(Policy = TenantAuthorizationPolicies.Manage)]
[Tags("Tenancy.Tenant")]
public sealed class TenantV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="TenantV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public TenantV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Provisions a new platform tenant end-to-end (Phase 7 §2.1).</summary>
    /// <param name="request">Caller-supplied provisioning fields.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TenantProvisioningResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TenantProvisioningResultDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ProvisionAsync([FromBody] ProvisionTenantRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = ProvisionTenantCommand.FromDto(
            actorId: env.ActorId,
            name: request.Name,
            region: request.Region,
            planCode: request.PlanCode,
            adminEmail: request.AdminEmail,
            adminFirstName: request.AdminFirstName,
            adminLastName: request.AdminLastName,
            correlationId: env.CorrelationId,
            causationId: env.CausationId,
            apiVersionToken: env.ApiVersionToken);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retrieves a single tenant by identity.</summary>
    /// <param name="tenantId">Identity of the tenant to retrieve.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{tenantId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TenantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetTenantQuery(tenantId);
        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Lists every registered tenant, most recently created first.</summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TenantDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var result = await _sender.Send(new GetTenantListQuery(), cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Suspends an Active tenant.</summary>
    /// <param name="tenantId">Identity of the tenant to suspend.</param>
    /// <param name="request">Suspension reason.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("{tenantId:guid}/suspend")]
    [ProducesResponseType(typeof(ApiResponse<TenantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TenantDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SuspendAsync(Guid tenantId, [FromBody] SuspendTenantRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = SuspendTenantCommand.FromDto(
            tenantId, env.ActorId, request.Reason, env.CorrelationId, env.CausationId, env.ApiVersionToken);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Reactivates a Suspended tenant back to Active.</summary>
    /// <param name="tenantId">Identity of the tenant to reactivate.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("{tenantId:guid}/reactivate")]
    [ProducesResponseType(typeof(ApiResponse<TenantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TenantDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReactivateAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = ReactivateTenantCommand.FromDto(tenantId, env.ActorId, env.CorrelationId, env.CausationId, env.ApiVersionToken);
        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Soft-deletes a tenant. Never a hard DELETE, per this codebase's canonical soft-delete pattern.</summary>
    /// <param name="tenantId">Identity of the tenant to delete.</param>
    /// <param name="request">Deletion reason.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpDelete("{tenantId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TenantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TenantDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteAsync(Guid tenantId, [FromBody] DeleteTenantRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = DeleteTenantCommand.FromDto(
            tenantId, env.ActorId, request.Reason, env.CorrelationId, env.CausationId, env.ApiVersionToken);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a tenant's data, transitioning it to Archived status.</summary>
    /// <param name="tenantId">Identity of the tenant whose data should be archived.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("{tenantId:guid}/archive")]
    [ProducesResponseType(typeof(ApiResponse<TenantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TenantDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = ArchiveTenantDataCommand.FromDto(tenantId, env.ActorId, env.CorrelationId, env.CausationId, env.ApiVersionToken);
        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Exports a tenant's own canonical Tenancy-context record as a downloadable JSON artifact.</summary>
    /// <param name="tenantId">Identity of the tenant whose record should be exported.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("{tenantId:guid}/export")]
    [ProducesResponseType(typeof(ApiResponse<TenantDataExportResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TenantDataExportResultDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = ExportTenantDataCommand.FromDto(tenantId, env.ActorId, env.CorrelationId, env.CausationId, env.ApiVersionToken);
        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
