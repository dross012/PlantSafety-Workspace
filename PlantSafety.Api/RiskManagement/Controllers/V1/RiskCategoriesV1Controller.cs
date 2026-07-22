// ============================================================================
// File: RiskCategoriesV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code modular-creation-agent)
// Created: 2026-07-21
// Description: V1 MVC controller exposing the 11 RiskManagement category read-model queries
//              over HTTP as one GET action per category.
// ============================================================================

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.RiskManagement.Contracts.Requests;
using PlantSafety.Application.RiskManagement.ChemicalEnvironmentalSafety.Queries;
using PlantSafety.Application.RiskManagement.ContinuousMonitoringReview.Queries;
using PlantSafety.Application.RiskManagement.ControlMeasuresHierarchy.Queries;
using PlantSafety.Application.RiskManagement.EmergencyPreparedness.Queries;
using PlantSafety.Application.RiskManagement.ErgonomicsMaterialHandling.Queries;
using PlantSafety.Application.RiskManagement.HazardIdentificationMeasurement.Queries;
using PlantSafety.Application.RiskManagement.LotoConfinedSpaces.Queries;
using PlantSafety.Application.RiskManagement.MachineOperationalGuarding.Queries;
using PlantSafety.Application.RiskManagement.PreStartupSafetyReview.Queries;
using PlantSafety.Application.RiskManagement.ProcessSafetyInformation.Queries;
using PlantSafety.Application.RiskManagement.TrainingContractorManagement.Queries;

namespace PlantSafety.Api.RiskManagement.Controllers.V1;

/// <summary>V1 controller for the 11 RiskManagement category read-model queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/riskmanagement/categories")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("RiskManagement")]
public sealed class RiskCategoriesV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="RiskCategoriesV1Controller"/>.</summary>
    public RiskCategoriesV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Returns ChemicalEnvironmentalSafety read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("chemical-environmental-safety")]
    [ProducesResponseType(typeof(ApiResponse<ChemicalEnvironmentalSafetyQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetChemicalEnvironmentalSafetyAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetChemicalEnvironmentalSafetyQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns ContinuousMonitoringReview read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("continuous-monitoring-review")]
    [ProducesResponseType(typeof(ApiResponse<ContinuousMonitoringReviewQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetContinuousMonitoringReviewAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetContinuousMonitoringReviewQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns ControlMeasuresHierarchy read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("control-measures-hierarchy")]
    [ProducesResponseType(typeof(ApiResponse<ControlMeasuresHierarchyQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetControlMeasuresHierarchyAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetControlMeasuresHierarchyQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns EmergencyPreparedness read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("emergency-preparedness")]
    [ProducesResponseType(typeof(ApiResponse<EmergencyPreparednessQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEmergencyPreparednessAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetEmergencyPreparednessQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns ErgonomicsMaterialHandling read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("ergonomics-material-handling")]
    [ProducesResponseType(typeof(ApiResponse<ErgonomicsMaterialHandlingQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetErgonomicsMaterialHandlingAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetErgonomicsMaterialHandlingQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns HazardIdentificationMeasurement read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("hazard-identification-measurement")]
    [ProducesResponseType(typeof(ApiResponse<HazardIdentificationMeasurementQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHazardIdentificationMeasurementAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetHazardIdentificationMeasurementQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns LotoConfinedSpaces read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("loto-confined-spaces")]
    [ProducesResponseType(typeof(ApiResponse<LotoConfinedSpacesQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLotoConfinedSpacesAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetLotoConfinedSpacesQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns MachineOperationalGuarding read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("machine-operational-guarding")]
    [ProducesResponseType(typeof(ApiResponse<MachineOperationalGuardingQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMachineOperationalGuardingAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetMachineOperationalGuardingQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns PreStartupSafetyReview read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("pre-startup-safety-review")]
    [ProducesResponseType(typeof(ApiResponse<PreStartupSafetyReviewQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPreStartupSafetyReviewAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPreStartupSafetyReviewQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns ProcessSafetyInformation read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("process-safety-information")]
    [ProducesResponseType(typeof(ApiResponse<ProcessSafetyInformationQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetProcessSafetyInformationAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetProcessSafetyInformationQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns TrainingContractorManagement read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("training-contractor-management")]
    [ProducesResponseType(typeof(ApiResponse<TrainingContractorManagementQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTrainingContractorManagementAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetTrainingContractorManagementQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
