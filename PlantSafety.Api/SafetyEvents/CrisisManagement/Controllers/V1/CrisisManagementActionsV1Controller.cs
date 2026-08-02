// ============================================================================
// File: CrisisManagementActionsV1Controller.cs
// Project: PlantSafety.Api
// Module: SafetyEvents.CrisisManagement
// Created: 2026-08-02
// Description: V1 controller exposing the Application-layer SafetyEvents.CrisisManagement
//              module's write commands and by-id query over HTTP. Covers the full
//              CrisisManagement lifecycle: Declare -> RespondTo -> Stabilize -> Resolve -> Close
//              -> Archive. Mirrors
//              PlantSafety.Api.Operational.PredictiveMaintenance.Controllers.V1
//              .PredictiveMaintenanceAlertV1Controller.
//
// Route note: routed under "api/v{version}/safety-events/crisis-management/actions" -- a
// distinct nested segment from the pre-existing read-only CrisisManagementV1Controller
// ("api/v{version}/safety-events/crisis-management", the date-range list endpoint backing
// GetCrisisManagementQuery, left completely untouched by this pass). The two controllers
// therefore never collide on route + verb even though they share the same module segment prefix.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields (CommandId/CorrelationId/TenantId/ActorId etc.) are populated via the shared
//   RequestEnvelopeFactory rather than hand-rolled per action. ActorId always comes from the
//   authenticated caller (ICurrentUserService), never from client-supplied input.
// - All mutating actions use SafetyTierPolicies.Write; reads use SafetyTierPolicies.Read.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.SafetyEvents.CrisisManagement.Contracts.Requests;
using PlantSafety.Application.SafetyEvents.CrisisManagement.Commands;
using PlantSafety.Application.SafetyEvents.CrisisManagement.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.SafetyEvents.CrisisManagement.Controllers.V1;

/// <summary>V1 controller for the SafetyEvents.CrisisManagement module's write commands and by-id query.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/safety-events/crisis-management/actions")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("SafetyEvents.CrisisManagement")]
public sealed class CrisisManagementActionsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="CrisisManagementActionsV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public CrisisManagementActionsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Declares a new CrisisManagement crisis.</summary>
    [HttpPost]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CrisisManagementStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CrisisManagementStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeclareAsync(
        [FromBody] DeclareCrisisManagementRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new DeclareCrisisManagementCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            Title: request.Title,
            Severity: request.Severity,
            DeclaredAtUtc: request.DeclaredAtUtc,
            DeclaredByUserId: env.ActorId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a response to a declared crisis (Declared -&gt; Active).</summary>
    [HttpPost("{crisisManagementId:guid}/respond-to")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CrisisManagementStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CrisisManagementStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RespondToAsync(
        Guid crisisManagementId,
        [FromBody] CrisisManagementLifecycleRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RespondToCrisisManagementCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            CrisisManagementId: crisisManagementId,
            RespondedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records that an active crisis has been stabilized.</summary>
    [HttpPost("{crisisManagementId:guid}/stabilize")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CrisisManagementStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CrisisManagementStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> StabilizeAsync(
        Guid crisisManagementId,
        [FromBody] CrisisManagementLifecycleRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new StabilizeCrisisManagementCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            CrisisManagementId: crisisManagementId,
            StabilizedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Resolves an active crisis (Active -&gt; Completed).</summary>
    [HttpPost("{crisisManagementId:guid}/resolve")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CrisisManagementStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CrisisManagementStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResolveAsync(
        Guid crisisManagementId,
        [FromBody] CrisisManagementLifecycleRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ResolveCrisisManagementCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            CrisisManagementId: crisisManagementId,
            ResolvedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Formally closes a resolved crisis (Completed -&gt; Closed).</summary>
    [HttpPost("{crisisManagementId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CrisisManagementStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CrisisManagementStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CloseAsync(
        Guid crisisManagementId,
        [FromBody] CrisisManagementLifecycleRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CloseCrisisManagementCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            CrisisManagementId: crisisManagementId,
            ClosedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Archives a closed crisis record (Closed -&gt; Archived).</summary>
    [HttpPost("{crisisManagementId:guid}/archive")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<CrisisManagementStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CrisisManagementStatusResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ArchiveAsync(
        Guid crisisManagementId,
        [FromBody] CrisisManagementLifecycleRequest? request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new ArchiveCrisisManagementCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            TenantId: env.TenantId,
            CrisisManagementId: crisisManagementId,
            ArchivedByUserId: env.ActorId)
        {
            ETag = request?.ETag
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single CrisisManagement read-model record by identity, including its current ETag.</summary>
    [HttpGet("{crisisManagementId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.SafetyEvents.CrisisManagement.Models.CrisisManagementDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetByIdAsync(
        Guid crisisManagementId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetCrisisManagementByIdQuery(env.TenantId, crisisManagementId);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
