// ============================================================================
// File: HazardousWasteManagementV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-04
// Description: V1 controller exposing every HazardousWasteManagement module command
//              (WasteStreams, AccumulationAreas/WasteContainers, Manifests, DisposalCertificates)
//              and their read-model queries over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/hazardouswastemanagement/{sub-component}/...
// - Commands/queries return PlantSafety.Application.Common.Results.Result<T>, mapped to
//   ApiResponse<T> via PlantSafety.Api.Common.ResultResponseExtensions.ToActionResult.
// - Commands are positional records: (CommandId, CorrelationId, CausationId, TenantId, <Entity>Id,
//   ...domain fields..., PerformedByUserId). PerformedByUserId is always populated from the current
//   actor (env.ActorId); the aggregate id is always bound from the route, never the body.
// - Discrepancy flags and time-limit breaches are expected to escalate to RiskManagement/Alerts via
//   the domain event stream (WasteContainerTimeLimitPolicy, ManifestDiscrepancyFlaggedEvent), not
//   directly from this controller.
//
// ============================================================================

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.HazardousWasteManagement.Contracts.Requests;
using PlantSafety.Application.HazardousWasteManagement.Commands;
using PlantSafety.Application.HazardousWasteManagement.AccumulationAreas.Queries;
using PlantSafety.Application.HazardousWasteManagement.DisposalCertificates.Queries;
using PlantSafety.Application.HazardousWasteManagement.Manifests.Queries;
using PlantSafety.Application.HazardousWasteManagement.WasteContainers.Queries;
using PlantSafety.Application.HazardousWasteManagement.WasteStreams.Queries;

namespace PlantSafety.Api.HazardousWasteManagement.Controllers.V1;

/// <summary>V1 controller for the HazardousWasteManagement module's commands and read-model queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/hazardouswastemanagement")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("HazardousWasteManagement")]
public sealed class HazardousWasteManagementV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="HazardousWasteManagementV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public HazardousWasteManagementV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    // ══════════════════════════════════════════════════════════════════════
    // WasteStreams
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Characterizes a new hazardous waste stream.</summary>
    /// <response code="200">Waste stream characterized.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("waste-streams")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<WasteStreamActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WasteStreamActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CharacterizeWasteStreamAsync([FromBody] CharacterizeWasteStreamRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new CharacterizeWasteStreamCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.StreamName, request.EpaWasteCode, request.HazardClass, request.GeneratorEpaId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single WasteStream record by identifier.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("waste-streams/{wasteStreamId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<GetWasteStreamByIdResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetWasteStreamByIdAsync(Guid wasteStreamId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetWasteStreamByIdQuery(env.TenantId, wasteStreamId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // AccumulationAreas / WasteContainers
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Opens a new accumulation container, starting the RCRA-style 90/180-day accumulation clock.</summary>
    /// <response code="200">Container opened.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("accumulation-containers")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<WasteContainerActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WasteContainerActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> OpenAccumulationContainerAsync([FromBody] OpenAccumulationContainerRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new OpenAccumulationContainerCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.AccumulationAreaId, request.WasteStreamId, request.ContainerLabel,
            request.CapacityGallons, request.RegulatoryTimeLimitDays, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Closes an accumulation container.</summary>
    /// <response code="200">Container closed.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("accumulation-containers/{wasteContainerId:guid}/close")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<WasteContainerActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<WasteContainerActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<WasteContainerActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseAccumulationContainerAsync(Guid wasteContainerId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new CloseAccumulationContainerCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            wasteContainerId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Lists accumulation containers within a given accumulation area.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("accumulation-areas/{accumulationAreaId:guid}/containers")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ListContainersByAccumulationAreaResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListContainersByAccumulationAreaAsync(Guid accumulationAreaId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new ListContainersByAccumulationAreaQuery(env.TenantId, accumulationAreaId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Lists accumulation areas with containers approaching their regulatory time limit. Feeds the Accumulation Area Time-Limit Board dashboard.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("accumulation-areas/nearing-time-limit")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ListAccumulationAreasNearingTimeLimitResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAccumulationAreasNearingTimeLimitAsync(
        [FromQuery] HazardousWasteManagementSiteQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new ListAccumulationAreasNearingTimeLimitQuery(env.TenantId, request.SiteId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // Manifests
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Generates a new Uniform Hazardous Waste Manifest.</summary>
    /// <response code="200">Manifest generated.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("manifests")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<HazardousWasteManifestActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazardousWasteManifestActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GenerateManifestAsync([FromBody] GenerateManifestRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new GenerateManifestCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.ManifestNumber, request.GeneratorEpaId, request.WasteStreamId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records transporter pickup/custody transfer. From this point the manifest becomes immutable.</summary>
    /// <response code="200">Pickup recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("manifests/{hazardousWasteManifestId:guid}/record-transporter-pickup")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<HazardousWasteManifestActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazardousWasteManifestActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<HazardousWasteManifestActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordTransporterPickupAsync(
        Guid hazardousWasteManifestId, [FromBody] RecordTransporterPickupRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordTransporterPickupCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            hazardousWasteManifestId, request.TransporterId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records TSDF facility receipt.</summary>
    /// <response code="200">Receipt recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("manifests/{hazardousWasteManifestId:guid}/record-tsdf-receipt")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<HazardousWasteManifestActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazardousWasteManifestActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<HazardousWasteManifestActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordTsdfReceiptAsync(
        Guid hazardousWasteManifestId, [FromBody] RecordTsdfReceiptRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordTsdfReceiptCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            hazardousWasteManifestId, request.TsdfFacilityId, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Flags a discrepancy against a manifest. Triggers downstream Alerts.</summary>
    /// <response code="200">Discrepancy flagged.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Record not found.</response>
    [HttpPost("manifests/{hazardousWasteManifestId:guid}/flag-discrepancy")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<HazardousWasteManifestActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HazardousWasteManifestActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<HazardousWasteManifestActionResult>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FlagManifestDiscrepancyAsync(
        Guid hazardousWasteManifestId, [FromBody] FlagManifestDiscrepancyRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new FlagManifestDiscrepancyCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            hazardousWasteManifestId, request.DiscrepancyReason, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single manifest record by identifier.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("manifests/{hazardousWasteManifestId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<GetManifestByIdResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetManifestByIdAsync(Guid hazardousWasteManifestId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetManifestByIdQuery(env.TenantId, hazardousWasteManifestId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Lists all manifests not yet Closed. Feeds the Hazardous Waste Lifecycle Tracker dashboard.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("manifests/open")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<ListOpenManifestsResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListOpenManifestsAsync([FromQuery] HazardousWasteManagementSiteQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new ListOpenManifestsQuery(env.TenantId, request.SiteId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ══════════════════════════════════════════════════════════════════════
    // DisposalCertificates
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>Records a disposal certificate received from a TSDF facility, closing the chain-of-custody loop.</summary>
    /// <response code="200">Disposal certificate recorded.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost("disposal-certificates")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<DisposalCertificateActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DisposalCertificateActionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordDisposalCertificateAsync([FromBody] RecordDisposalCertificateRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();
        var command = new RecordDisposalCertificateCommand(
            env.CommandId, env.CorrelationId, env.CausationId ?? Guid.NewGuid(), env.TenantId,
            request.SiteId, request.ManifestId, request.TsdfFacilityId, request.DisposalMethod, request.DisposalDateUtc, env.ActorId);
        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the disposal certificate history for a given waste stream.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("waste-streams/{wasteStreamId:guid}/disposal-history")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<GetDisposalHistoryByStreamResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDisposalHistoryByStreamAsync(Guid wasteStreamId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetDisposalHistoryByStreamQuery(env.TenantId, wasteStreamId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
}
