// ============================================================================
// File: FinancialRecordV1Controller.cs
// Project: PlantSafety.Api
// Module: Financials
// Created: 2026-08-02
// Description: V1 controller exposing the Application-layer FinancialRecord commands and queries
//              over HTTP. Covers the full lifecycle: Create -> AddLineItem -> Submit -> Approve /
//              Reject -> Close -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Auditing.Controllers.V1.AuditingV1Controller.
//
// Route note: this controller's route ("api/v{version}/financial-records") is deliberately
// distinct from the pre-existing, unrelated FinancialsV1Controller's route
// ("api/v{version}/financials"), confirmed by grep before finalizing this route -- no collision.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields are populated via the shared RequestEnvelopeFactory. ActorId always comes
//   from the authenticated caller (ICurrentUserService), never from client-supplied input.
// - All mutating actions use SafetyTierPolicies.Write; reads use SafetyTierPolicies.Read.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Financials.Contracts.Requests;
using PlantSafety.Application.Financials.Commands;
using PlantSafety.Application.Financials.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Financials.Controllers.V1;

/// <summary>V1 controller for the new FinancialRecord aggregate's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/financial-records")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Financials.FinancialRecord")]
public sealed class FinancialRecordV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="FinancialRecordV1Controller"/>.</summary>
    public FinancialRecordV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new FinancialRecord (Status=Draft).</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateFinancialRecordResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateFinancialRecordResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateFinancialRecordRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateFinancialRecordCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            Category: request.Category,
            Amount: request.Amount,
            RequesterUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Appends a new line item to a Draft FinancialRecord.</summary>
    [HttpPost("{financialRecordId:guid}/line-items")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<FinancialRecordStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FinancialRecordStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddLineItemAsync(
        Guid financialRecordId,
        [FromBody] AddFinancialRecordLineItemRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AddFinancialRecordLineItemCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            FinancialRecordId: financialRecordId,
            Description: request.Description,
            Amount: request.Amount,
            ActorUserId: env.ActorId)
        {
            ETag = request.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Submits a FinancialRecord for approval (Draft -&gt; Submitted).</summary>
    [HttpPost("{financialRecordId:guid}/submit")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<FinancialRecordStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FinancialRecordStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SubmitAsync(
        Guid financialRecordId,
        [FromBody] SubmitFinancialRecordRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new SubmitFinancialRecordCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            FinancialRecordId: financialRecordId,
            ActorUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Approves a FinancialRecord (Submitted -&gt; Approved).</summary>
    [HttpPost("{financialRecordId:guid}/approve")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<FinancialRecordStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FinancialRecordStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ApproveAsync(
        Guid financialRecordId,
        [FromBody] ApproveFinancialRecordRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ApproveFinancialRecordCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            FinancialRecordId: financialRecordId,
            ActorUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Rejects a FinancialRecord (Submitted -&gt; Rejected).</summary>
    [HttpPost("{financialRecordId:guid}/reject")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<FinancialRecordStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FinancialRecordStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RejectAsync(
        Guid financialRecordId,
        [FromBody] RejectFinancialRecordRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RejectFinancialRecordCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            FinancialRecordId: financialRecordId,
            Reason: request?.Reason,
            ActorUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Closes a FinancialRecord (Approved -&gt; Closed).</summary>
    [HttpPost("{financialRecordId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<FinancialRecordStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FinancialRecordStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid financialRecordId,
        [FromBody] CloseFinancialRecordRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseFinancialRecordCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            FinancialRecordId: financialRecordId,
            ActorUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a FinancialRecord (Closed or Rejected -&gt; Archived).</summary>
    [HttpPost("{financialRecordId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<FinancialRecordStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<FinancialRecordStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid financialRecordId,
        [FromBody] ArchiveFinancialRecordRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveFinancialRecordCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            FinancialRecordId: financialRecordId,
            ActorUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single FinancialRecord read-model record by identity, including its line items and current ETag.</summary>
    [HttpGet("{financialRecordId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Financials.Models.FinancialRecordDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid financialRecordId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new FinancialRecordByIdQuery(env.TenantId, financialRecordId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
