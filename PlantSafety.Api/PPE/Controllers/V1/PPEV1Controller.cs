// ============================================================================
// File: PPEV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: V1 controller exposing the Application-layer PPE module's Commands and Queries
//              over HTTP (PPEAssignment, PPECategory, PPEInventory, PPETrainingFit,
//              PPEZoneMapping).
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/ppe/...
// - Envelope fields (CommandId/CorrelationId/TenantId/ActorId etc.) are populated via the
//   shared RequestEnvelopeFactory rather than hand-rolled per action. ActorId always comes
//   from the authenticated caller (ICurrentUserService), never from client-supplied input.
//
// Rationale:
// - Routes all reads/writes through MediatR ISender to the Application layer. No domain
//   logic or data access here.
//
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.PPE.Contracts.Requests;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PPE.Commands;
using PlantSafety.Application.PPE.PPEAssignment.Queries;
using PlantSafety.Application.PPE.PPECategory.Queries;
using PlantSafety.Application.PPE.PPEInventory.Queries;
using PlantSafety.Application.PPE.PPETrainingFit.Queries;
using PlantSafety.Application.PPE.PPEZoneMapping.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.PPE.Controllers.V1;

/// <summary>V1 controller for the PPE module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ppe")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("PPE")]
public sealed class PPEV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="PPEV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public PPEV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    // ── PPEAssignment ────────────────────────────────────────────────────────

    /// <summary>Issues the assigned PPE to the worker.</summary>
    [HttpPost("assignments/{ppeAssignmentId:guid}/assign")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<PPEAssignmentActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PPEAssignmentActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignAsync(
        Guid ppeAssignmentId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AssignPPECommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PPEAssignmentId: ppeAssignmentId,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Flags a PPEAssignment for replacement.</summary>
    [HttpPost("assignments/{ppeAssignmentId:guid}/flag-for-replacement")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<PPEAssignmentActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PPEAssignmentActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> FlagPPEAssignmentForReplacementAsync(
        Guid ppeAssignmentId,
        [FromBody] FlagForReplacementRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new FlagPPEAssignmentForReplacementCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PPEAssignmentId: ppeAssignmentId,
            Reason: request.Reason,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires a PPEAssignment.</summary>
    [HttpPost("assignments/{ppeAssignmentId:guid}/retire")]
    [Authorize(Policy = SafetyTierPolicies.Approve)]
    [ProducesResponseType(typeof(ApiResponse<PPEAssignmentActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PPEAssignmentActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RetirePPEAssignmentAsync(
        Guid ppeAssignmentId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RetirePPEAssignmentCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PPEAssignmentId: ppeAssignmentId,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a fit test result for a PPEAssignment.</summary>
    [HttpPost("assignments/{ppeAssignmentId:guid}/record-fit-test")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PPEAssignmentActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PPEAssignmentActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordPPEFitTestAsync(
        Guid ppeAssignmentId,
        [FromBody] RecordPPEFitTestRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RecordPPEFitTestCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PPEAssignmentId: ppeAssignmentId,
            Passed: request.Passed,
            Notes: request.Notes,
            FitTestExpiresAtUtc: request.FitTestExpiresAtUtc,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns PPEAssignment read-model data for a tenant/site within a date range.</summary>
    [HttpGet("assignments")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PPEAssignmentQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PPEAssignmentQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPPEAssignmentAsync(
        [FromQuery] PPEListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPPEAssignmentQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── PPECategory ──────────────────────────────────────────────────────────

    /// <summary>Returns PPECategory read-model data for a tenant/site within a date range.</summary>
    [HttpGet("category")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PPECategoryQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PPECategoryQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPPECategoryAsync(
        [FromQuery] PPEListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPPECategoryQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── PPEInventory ─────────────────────────────────────────────────────────

    /// <summary>Receives new stock into a PPEInventory item.</summary>
    [HttpPost("inventory/{ppeInventoryId:guid}/receive-stock")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PPEInventoryActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PPEInventoryActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReceivePPEStockAsync(
        Guid ppeInventoryId,
        [FromBody] ReceivePPEStockRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ReceivePPEStockCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PPEInventoryId: ppeInventoryId,
            Quantity: request.Quantity,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Issues units of PPE out of a PPEInventory item's on-hand stock.</summary>
    [HttpPost("inventory/{ppeInventoryId:guid}/issue-units")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PPEInventoryActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PPEInventoryActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> IssuePPEUnitsAsync(
        Guid ppeInventoryId,
        [FromBody] IssuePPEUnitsRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new IssuePPEUnitsCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PPEInventoryId: ppeInventoryId,
            Quantity: request.Quantity,
            ReorderThreshold: request.ReorderThreshold,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Flags a PPEInventory item for replacement.</summary>
    [HttpPost("inventory/{ppeInventoryId:guid}/flag-for-replacement")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PPEInventoryActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PPEInventoryActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> FlagPPEInventoryForReplacementAsync(
        Guid ppeInventoryId,
        [FromBody] FlagForReplacementRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new FlagPPEInventoryForReplacementCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PPEInventoryId: ppeInventoryId,
            Reason: request.Reason,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires a PPEInventory item from active service.</summary>
    [HttpPost("inventory/{ppeInventoryId:guid}/retire")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<PPEInventoryActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PPEInventoryActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RetirePPEInventoryAsync(
        Guid ppeInventoryId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RetirePPEInventoryCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            PPEInventoryId: ppeInventoryId,
            PerformedByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns PPEInventory read-model data for a tenant/site within a date range.</summary>
    [HttpGet("inventory")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PPEInventoryQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PPEInventoryQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPPEInventoryAsync(
        [FromQuery] PPEListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPPEInventoryQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── PPETrainingFit ───────────────────────────────────────────────────────

    /// <summary>Returns PPETrainingFit read-model data for a tenant/site within a date range.</summary>
    [HttpGet("training-fit")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PPETrainingFitQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PPETrainingFitQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPPETrainingFitAsync(
        [FromQuery] PPEListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPPETrainingFitQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── PPEZoneMapping ───────────────────────────────────────────────────────

    /// <summary>Returns PPEZoneMapping read-model data for a tenant/site within a date range.</summary>
    [HttpGet("zone-mapping")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PPEZoneMappingQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PPEZoneMappingQueryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPPEZoneMappingAsync(
        [FromQuery] PPEListRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPPEZoneMappingQuery(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            From: request.From ?? DateTime.UtcNow.AddDays(-90),
            To: request.To ?? DateTime.UtcNow.AddDays(90));

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── Bulk ─────────────────────────────────────────────────────────────────

    /// <summary>Applies a single operation uniformly across multiple PPE resources.</summary>
    [HttpPost("bulk")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<BulkUpdatePPEResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BulkUpdatePPEResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> BulkUpdateAsync(
        [FromBody] BulkUpdatePPERequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new BulkUpdatePPECommand(
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            ResourceIds: request.ResourceIds,
            Operation: request.Operation,
            Parameters: request.Parameters)
        {
            CommandId = env.CommandId,
            CorrelationId = env.CorrelationId,
            CausationId = env.CausationId,
            RequestedAtUtc = env.RequestedAtUtc,
            ApiVersionToken = env.ApiVersionToken,
            ActorId = env.ActorId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

}
