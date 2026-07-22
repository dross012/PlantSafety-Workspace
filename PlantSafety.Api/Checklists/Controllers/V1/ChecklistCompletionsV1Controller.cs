// ============================================================================
// File: ChecklistCompletionsV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: V1 controller exposing the Checklists module's ChecklistCompletion commands over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/checklists/completions/...
//
// ============================================================================

using PlantSafety.Api.Checklists.Contracts.Requests;
using PlantSafety.Api.Context;
using PlantSafety.Application.Checklists.Commands;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Checklists.Controllers.V1;

/// <summary>V1 controller for Checklist Completion commands.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/checklists/completions")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Checklists")]
public sealed class ChecklistCompletionsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ChecklistCompletionsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public ChecklistCompletionsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new ChecklistCompletion.</summary>
    /// <response code="200">Checklist completion created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateChecklistCompletionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateChecklistCompletionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CreateChecklistCompletionResult>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateChecklistCompletionRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateChecklistCompletionCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            OwnerId: env.ActorId,
            ChecklistTemplateId: request.ChecklistTemplateId,
            ItemsTotal: request.ItemsTotal,
            ItemsPassed: request.ItemsPassed,
            ItemsFailed: request.ItemsFailed,
            Outcome: request.Outcome);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Starts a ChecklistCompletion.</summary>
    /// <response code="200">Checklist completion started successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Checklist completion not found.</response>
    [HttpPost("{checklistCompletionId:guid}/start")]
    [ProducesResponseType(typeof(ApiResponse<StartChecklistCompletionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StartChecklistCompletionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<StartChecklistCompletionResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StartAsync(
        Guid checklistCompletionId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new StartChecklistCompletionCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            OwnerId: env.ActorId,
            ChecklistCompletionId: checklistCompletionId)
        {
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records the pass/fail result for a single checklist item within a ChecklistCompletion.</summary>
    /// <response code="200">Item result recorded successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Checklist completion not found.</response>
    [HttpPost("{checklistCompletionId:guid}/items/{checklistTemplateItemId:guid}/result")]
    [ProducesResponseType(typeof(ApiResponse<RecordChecklistItemResultResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RecordChecklistItemResultResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RecordChecklistItemResultResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordItemResultAsync(
        Guid checklistCompletionId,
        Guid checklistTemplateItemId,
        [FromBody] RecordChecklistItemResultRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RecordChecklistItemResultCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            OwnerId: env.ActorId,
            ChecklistCompletionId: checklistCompletionId,
            ChecklistTemplateItemId: checklistTemplateItemId,
            Passed: request.Passed,
            Notes: request.Notes)
        {
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Flags a checklist item as a critical failure within a ChecklistCompletion.</summary>
    /// <response code="200">Item flagged successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Checklist completion not found.</response>
    [HttpPost("{checklistCompletionId:guid}/items/{checklistTemplateItemId:guid}/flag-failed")]
    [ProducesResponseType(typeof(ApiResponse<FlagFailedChecklistItemResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FlagFailedChecklistItemResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<FlagFailedChecklistItemResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> FlagFailedAsync(
        Guid checklistCompletionId,
        Guid checklistTemplateItemId,
        [FromBody] FlagFailedChecklistItemRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new FlagFailedChecklistItemCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            OwnerId: env.ActorId,
            ChecklistCompletionId: checklistCompletionId,
            ChecklistTemplateItemId: checklistTemplateItemId,
            Description: request.Description)
        {
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Completes a ChecklistCompletion.</summary>
    /// <response code="200">Checklist completion completed successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Checklist completion not found.</response>
    [HttpPost("{checklistCompletionId:guid}/complete")]
    [ProducesResponseType(typeof(ApiResponse<CompleteChecklistCompletionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CompleteChecklistCompletionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CompleteChecklistCompletionResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompleteAsync(
        Guid checklistCompletionId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteChecklistCompletionCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            OwnerId: env.ActorId,
            ChecklistCompletionId: checklistCompletionId)
        {
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
