// ============================================================================
// File: EpaComplianceV1Controller.cs
// Project: PlantSafety.Api
// Module: Compliance.EPA.Compliance
// Created: 2026-08-07
// Description: V1 controller exposing the Application-layer Compliance.EPA.Compliance module's
//              five EPA compliance-evaluation orchestration commands over HTTP: each wraps one of
//              the pure Domain-layer EPA compliance engines (RCRA, TSCA, CERCLA/SARA, Clean Air
//              Act, Clean Water Act). Uses the "Epa" prefix (not bare "EPA") throughout to avoid
//              collision with the pre-existing, unrelated Compliance.Certification.EPA /
//              Compliance.RegulatoryCalendar.Deadlines.EPA controllers (EPAV1Controller in each),
//              and with PlantSafety.Api.Compliance.EPA.Controllers.V1.EpaRequirementsV1Controller
//              (reference-data read-model, a parallel effort under a different route/subfolder).
//
//              POST api/v1/compliance/epa/compliance/rcra
//              POST api/v1/compliance/epa/compliance/tsca
//              POST api/v1/compliance/epa/compliance/cercla
//              POST api/v1/compliance/epa/compliance/clean-air
//              POST api/v1/compliance/epa/compliance/clean-water
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields (CommandId/CorrelationId/TenantId/ActorId etc.) are populated via the shared
//   RequestEnvelopeFactory rather than hand-rolled per action. ActorId always comes from the
//   authenticated caller (ICurrentUserService), never from client-supplied input.
// - These endpoints evaluate compliance but persist nothing themselves; per the same convention
//   as PHMSA's Classification/ShippingPapers controllers for non-trivial evaluation actions, they
//   are guarded by SafetyTierPolicies.Write (a meaningful compliance-evaluation action, not a
//   passive read of already-persisted data).
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Compliance.EPA.Compliance.Contracts.Requests;
using PlantSafety.Application.Compliance.EPA.Compliance.Commands;
using PlantSafety.Application.Compliance.EPA.Compliance.Models;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Compliance.EPA.Compliance.Controllers.V1;

/// <summary>V1 controller for the Compliance.EPA.Compliance module's EPA compliance-evaluation commands.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/epa/compliance")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Compliance.EPA.Compliance")]
public sealed class EpaComplianceV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="EpaComplianceV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public EpaComplianceV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Evaluates RCRA hazardous-waste-management compliance for a site's current waste containers and manifests.</summary>
    [HttpPost("rcra")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EpaComplianceReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EpaComplianceReportDto>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EvaluateRcraAsync(
        [FromBody] EpaRcraComplianceRequestDto request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new EvaluateRcraComplianceCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId,
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            ActorId: env.ActorId,
            SiteId: request.SiteId,
            AccumulationAreaIds: request.AccumulationAreaIds,
            MonthlyHazardousWasteQuantityKg: request.MonthlyHazardousWasteQuantityKg,
            MonthlyAcuteHazardousWasteQuantityKg: request.MonthlyAcuteHazardousWasteQuantityKg);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Evaluates TSCA chemical-substance compliance for a caller-supplied chemical inventory.</summary>
    [HttpPost("tsca")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EpaComplianceReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EpaComplianceReportDto>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EvaluateTscaAsync(
        [FromBody] EpaTscaComplianceRequestDto request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new EvaluateTscaComplianceCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId,
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            ActorId: env.ActorId,
            SiteId: request.SiteId,
            Chemicals: request.Chemicals);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Evaluates whether a hazardous-substance release event triggers CERCLA/EPCRA emergency release-reporting obligations.</summary>
    [HttpPost("cercla")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EpaComplianceReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EpaComplianceReportDto>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EvaluateCerclaAsync(
        [FromBody] EpaCerclaReportingRequestDto request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new EvaluateCerclaReportingCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId,
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            ActorId: env.ActorId,
            SiteId: request.SiteId,
            LinkedIncidentManagementId: request.LinkedIncidentManagementId,
            SubstanceName: request.SubstanceName,
            CasNumber: request.CasNumber,
            ReleasedQuantity: request.ReleasedQuantity,
            QuantityUnit: request.QuantityUnit,
            ReportableQuantityThreshold: request.ReportableQuantityThreshold,
            IsExtremelyHazardousSubstance: request.IsExtremelyHazardousSubstance,
            ReleaseOccurredAtUtc: request.ReleaseOccurredAtUtc);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Evaluates Clean Air Act compliance for a caller-supplied set of air emission sources.</summary>
    [HttpPost("clean-air")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EpaComplianceReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EpaComplianceReportDto>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EvaluateCleanAirAsync(
        [FromBody] EpaCleanAirComplianceRequestDto request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new EvaluateCleanAirComplianceCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId,
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            ActorId: env.ActorId,
            SiteId: request.SiteId,
            EmissionSources: request.EmissionSources);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Evaluates Clean Water Act compliance for a site's discharge permits, limit exceedances, and SPCC applicability.</summary>
    [HttpPost("clean-water")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<EpaComplianceReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EpaComplianceReportDto>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EvaluateCleanWaterAsync(
        [FromBody] EpaCleanWaterComplianceRequestDto request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new EvaluateCleanWaterComplianceCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId,
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            ActorId: env.ActorId,
            SiteId: request.SiteId,
            DischargePermitIds: request.DischargePermitIds,
            FacilityStoresOilAboveSpccThreshold: request.FacilityStoresOilAboveSpccThreshold,
            HasSpccPlan: request.HasSpccPlan,
            SpccPlanLastReviewedAtUtc: request.SpccPlanLastReviewedAtUtc);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
