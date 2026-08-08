// ============================================================================
// File: StandardsComplianceV1Controller.cs
// Project: PlantSafety.Api
// Module: Compliance.Standards.Compliance
// Created: 2026-08-07
// Description: V1 controller exposing the Application-layer Compliance.Standards.Compliance
//              module's six ANSI/ASME/ISO consensus-standards compliance-evaluation orchestration
//              commands over HTTP: each wraps one of the pure Domain-layer Standards compliance
//              engines (CraneAndHoist, ControlSystemSafety, MachineSafety, RiskAssessment,
//              OperationalSafety, PressureVessel).
//
//              POST api/v1/compliance/standards/cranes
//              POST api/v1/compliance/standards/control-systems
//              POST api/v1/compliance/standards/machine
//              POST api/v1/compliance/standards/risk-assessment
//              POST api/v1/compliance/standards/operational
//              POST api/v1/compliance/standards/pressure-vessels
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields (CommandId/CorrelationId/TenantId/ActorId etc.) are populated via the shared
//   RequestEnvelopeFactory rather than hand-rolled per action. ActorId always comes from the
//   authenticated caller (ICurrentUserService), never from client-supplied input.
// - These endpoints evaluate compliance but persist nothing themselves; per the same convention
//   as EPA's Compliance controller for non-trivial evaluation actions, they are guarded by
//   SafetyTierPolicies.Write (a meaningful compliance-evaluation action, not a passive read of
//   already-persisted data).
// - Route base "api/v1/compliance/standards" is a sibling of, and does not collide with, the
//   parallel Compliance.Standards reference-data query-side effort under a different subfolder
//   (PlantSafety.Application.Compliance.Standards, without the ".Compliance" suffix).
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Compliance.Standards.Compliance.Contracts.Requests;
using PlantSafety.Application.Compliance.Standards.Compliance.Commands;
using PlantSafety.Application.Compliance.Standards.Compliance.Models;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Compliance.Standards.Compliance.Controllers.V1;

/// <summary>V1 controller for the Compliance.Standards.Compliance module's ANSI/ASME/ISO consensus-standards compliance-evaluation commands.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/compliance/standards")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Compliance.Standards.Compliance")]
public sealed class StandardsComplianceV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="StandardsComplianceV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public StandardsComplianceV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Evaluates ASME B30 compliance for a tenant's registered crane/hoist assets.</summary>
    [HttpPost("cranes")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<StandardsComplianceReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StandardsComplianceReportDto>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EvaluateCranesAsync(
        [FromBody] StandardsCraneComplianceRequestDto request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new EvaluateCraneComplianceCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId,
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            ActorId: env.ActorId,
            SiteId: request.SiteId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Evaluates ISO 13849-1 / IEC 62061 control-system safety compliance for a site's real safety-relevant components.</summary>
    [HttpPost("control-systems")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<StandardsComplianceReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StandardsComplianceReportDto>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EvaluateControlSystemsAsync(
        [FromBody] StandardsControlSystemSafetyRequestDto request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new EvaluateControlSystemSafetyCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId,
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            ActorId: env.ActorId,
            SiteId: request.SiteId,
            Classifications: request.Classifications);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Evaluates ANSI B11/Z244.1/Z535/ISO 12100/14120/14119 machine-safety compliance for a site's real guards, LOTO systems, e-stop devices, and two-hand controls.</summary>
    [HttpPost("machine")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<StandardsComplianceReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StandardsComplianceReportDto>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EvaluateMachineAsync(
        [FromBody] StandardsMachineSafetyRequestDto request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new EvaluateMachineSafetyStandardsCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId,
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            ActorId: env.ActorId,
            SiteId: request.SiteId);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Evaluates whether a documented machinery risk assessment meets ISO 12100 / ANSI B11.0 structural requirements.</summary>
    [HttpPost("risk-assessment")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<StandardsComplianceReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StandardsComplianceReportDto>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EvaluateRiskAssessmentAsync(
        [FromBody] StandardsRiskAssessmentRequestDto request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new EvaluateRiskAssessmentCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId,
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            ActorId: env.ActorId,
            EquipmentIdentifier: request.EquipmentIdentifier,
            Methodology: request.Methodology,
            IdentifiedHazards: request.IdentifiedHazards,
            SeverityEstimated: request.SeverityEstimated,
            ExposureEstimated: request.ExposureEstimated,
            ProbabilityEstimated: request.ProbabilityEstimated,
            RiskReductionMeasures: request.RiskReductionMeasures,
            IsIntegratedSystem: request.IsIntegratedSystem,
            AssessmentCompletedAtUtc: request.AssessmentCompletedAtUtc);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Evaluates ANSI Z10/ISO 45001/A1264/Z117 operational-safety compliance for a site.</summary>
    [HttpPost("operational")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<StandardsComplianceReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StandardsComplianceReportDto>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EvaluateOperationalAsync(
        [FromBody] StandardsOperationalSafetyRequestDto request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new EvaluateOperationalSafetyStandardsCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId,
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            ActorId: env.ActorId,
            SiteId: request.SiteId,
            SafetyManagementSystem: request.SafetyManagementSystem,
            SurfaceHazards: request.SurfaceHazards);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Evaluates ASME BPVC pressure-vessel compliance for a caller-supplied vessel inventory.</summary>
    [HttpPost("pressure-vessels")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<StandardsComplianceReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StandardsComplianceReportDto>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EvaluatePressureVesselsAsync(
        [FromBody] StandardsPressureVesselComplianceRequestDto request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new EvaluatePressureVesselComplianceCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId,
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            ActorId: env.ActorId,
            Vessels: request.Vessels);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
