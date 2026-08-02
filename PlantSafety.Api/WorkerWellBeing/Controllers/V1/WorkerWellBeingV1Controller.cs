// ============================================================================
// File: WorkerWellBeingV1Controller.cs
// Project: PlantSafety.Api
// Module: WorkerWellBeing
// Created: 2026-08-02
// Description: V1 controller exposing the Application-layer WorkerWellBeing module's Commands and
//              Queries over HTTP. Covers the full lifecycle: Create -> Activate -> Complete ->
//              Close -> Archive. Mirrors
//              PlantSafety.Api.Compliance.Auditing.Controllers.V1.AuditingV1Controller.
//
// Route note: this controller's route ("api/v{version}/worker-well-being/check-ins") does not
// collide with the pre-existing, unrelated Features-vertical record-tracking controllers under
// PlantSafety.Api/Features/WorkerWellBeing.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields are populated via the shared RequestEnvelopeFactory. ActorId always comes
//   from the authenticated caller (ICurrentUserService), never from client-supplied input.
// - All mutating actions use SafetyTierPolicies.Write; reads use SafetyTierPolicies.Read.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.WorkerWellBeing.Contracts.Requests;
using PlantSafety.Application.WorkerWellBeing.Commands;
using PlantSafety.Application.WorkerWellBeing.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.WorkerWellBeing.Controllers.V1;

/// <summary>V1 controller for the WorkerWellBeing module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/worker-well-being/check-ins")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("WorkerWellBeing")]
public sealed class WorkerWellBeingV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="WorkerWellBeingV1Controller"/>.</summary>
    public WorkerWellBeingV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Submits a new WorkerWellBeing check-in record.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CreateWorkerWellBeingResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateWorkerWellBeingResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateWorkerWellBeingRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateWorkerWellBeingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            WorkerUserId: request.WorkerUserId,
            WellBeingRiskLevel: request.WellBeingRiskLevel,
            CheckInType: request.CheckInType,
            WellBeingScore: request.WellBeingScore,
            FollowUpRequired: request.FollowUpRequired,
            RequesterUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Activates a worker wellbeing check-in record (Created -&gt; Active).</summary>
    [HttpPost("{workerWellBeingId:guid}/activate")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<WorkerWellBeingStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkerWellBeingStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateAsync(
        Guid workerWellBeingId,
        [FromBody] ActivateWorkerWellBeingRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ActivateWorkerWellBeingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            WorkerWellBeingId: workerWellBeingId,
            ActivatedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Completes an active worker wellbeing check-in record (Active -&gt; Completed).</summary>
    [HttpPost("{workerWellBeingId:guid}/complete")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<WorkerWellBeingStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkerWellBeingStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompleteAsync(
        Guid workerWellBeingId,
        [FromBody] CompleteWorkerWellBeingRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CompleteWorkerWellBeingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            WorkerWellBeingId: workerWellBeingId,
            CompletedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes a completed worker wellbeing check-in record (Completed -&gt; Closed).</summary>
    [HttpPost("{workerWellBeingId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<WorkerWellBeingStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkerWellBeingStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid workerWellBeingId,
        [FromBody] CloseWorkerWellBeingRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseWorkerWellBeingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            WorkerWellBeingId: workerWellBeingId,
            ClosedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed worker wellbeing check-in record for compliance retention (Closed -&gt; Archived).</summary>
    [HttpPost("{workerWellBeingId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<WorkerWellBeingStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WorkerWellBeingStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid workerWellBeingId,
        [FromBody] ArchiveWorkerWellBeingRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveWorkerWellBeingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            WorkerWellBeingId: workerWellBeingId,
            PerformedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single WorkerWellBeing read-model record by identity, including its current ETag.</summary>
    [HttpGet("{workerWellBeingId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.WorkerWellBeing.Models.WorkerWellBeingItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid workerWellBeingId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetWorkerWellBeingByIdQuery(env.TenantId, workerWellBeingId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
