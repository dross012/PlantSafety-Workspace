// ============================================================================
// File: SafetyPolicyCategoriesV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Claude Code modular-creation-agent)
// Created: 2026-07-21
// Description: V1 MVC controller exposing the 17 SafetyPolicies category read-model queries
//              (one Get<Category>Query per PolicyCategory, all sharing the same
//              TenantId/SiteId/From/To shape backed by usp_SafetyPolicies_GetByCategory-style
//              repositories) over HTTP as one GET action per category.
// ============================================================================
//
// Onboarding Notes:
// - Every action shares the identical dispatch shape: build the category query from the
//   shared CategoryQueryRequest DTO + TenantId from RequestEnvelopeFactory, send via ISender,
//   translate the Result<T> via ResultResponseExtensions.ToActionResult. Kept as one action per
//   category (rather than a single ?category= endpoint) because each category has its own
//   strongly-typed Result/DTO shape.
//
// ============================================================================

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.SafetyPolicies.Contracts.Requests;
using PlantSafety.Application.SafetyPolicies.ConfinedSpaceEntry.Queries;
using PlantSafety.Application.SafetyPolicies.ContractorSafety.Queries;
using PlantSafety.Application.SafetyPolicies.ElectricalSafety.Queries;
using PlantSafety.Application.SafetyPolicies.EmergencyResponseEvacuation.Queries;
using PlantSafety.Application.SafetyPolicies.EnvironmentalChemicalSafety.Queries;
using PlantSafety.Application.SafetyPolicies.EquipmentMachineOperation.Queries;
using PlantSafety.Application.SafetyPolicies.HazardCommunicationAssessment.Queries;
using PlantSafety.Application.SafetyPolicies.HotWork.Queries;
using PlantSafety.Application.SafetyPolicies.HousekeepingErgonomics.Queries;
using PlantSafety.Application.SafetyPolicies.IncidentInvestigationCorrectiveAction.Queries;
using PlantSafety.Application.SafetyPolicies.MachineMaintenanceInspection.Queries;
using PlantSafety.Application.SafetyPolicies.MaterialHandlingStorage.Queries;
using PlantSafety.Application.SafetyPolicies.PersonalProtectiveEquipment.Queries;
using PlantSafety.Application.SafetyPolicies.SafetyCultureBehavioral.Queries;
using PlantSafety.Application.SafetyPolicies.SafetyTrainingReporting.Queries;
using PlantSafety.Application.SafetyPolicies.SecurityAccessControl.Queries;
using PlantSafety.Application.SafetyPolicies.WorkingAtHeights.Queries;

namespace PlantSafety.Api.SafetyPolicies.Controllers.V1;

/// <summary>V1 controller for the 17 SafetyPolicies category read-model queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/safetypolicies/categories")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("SafetyPolicies")]
public sealed class SafetyPolicyCategoriesV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="SafetyPolicyCategoriesV1Controller"/>.</summary>
    public SafetyPolicyCategoriesV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Returns ConfinedSpaceEntry policy read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("confined-space-entry")]
    [ProducesResponseType(typeof(ApiResponse<ConfinedSpaceEntryQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetConfinedSpaceEntryAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetConfinedSpaceEntryQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns ContractorSafety policy read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("contractor-safety")]
    [ProducesResponseType(typeof(ApiResponse<ContractorSafetyQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetContractorSafetyAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetContractorSafetyQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns ElectricalSafety policy read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("electrical-safety")]
    [ProducesResponseType(typeof(ApiResponse<ElectricalSafetyQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetElectricalSafetyAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetElectricalSafetyQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns EmergencyResponseEvacuation policy read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("emergency-response-evacuation")]
    [ProducesResponseType(typeof(ApiResponse<EmergencyResponseEvacuationQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEmergencyResponseEvacuationAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetEmergencyResponseEvacuationQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns EnvironmentalChemicalSafety policy read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("environmental-chemical-safety")]
    [ProducesResponseType(typeof(ApiResponse<EnvironmentalChemicalSafetyQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEnvironmentalChemicalSafetyAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetEnvironmentalChemicalSafetyQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns EquipmentMachineOperation policy read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("equipment-machine-operation")]
    [ProducesResponseType(typeof(ApiResponse<EquipmentMachineOperationQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEquipmentMachineOperationAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetEquipmentMachineOperationQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns HazardCommunicationAssessment policy read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("hazard-communication-assessment")]
    [ProducesResponseType(typeof(ApiResponse<HazardCommunicationAssessmentQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHazardCommunicationAssessmentAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetHazardCommunicationAssessmentQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns HotWork policy read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("hot-work")]
    [ProducesResponseType(typeof(ApiResponse<HotWorkQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHotWorkAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetHotWorkQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns HousekeepingErgonomics policy read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("housekeeping-ergonomics")]
    [ProducesResponseType(typeof(ApiResponse<HousekeepingErgonomicsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHousekeepingErgonomicsAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetHousekeepingErgonomicsQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns IncidentInvestigationCorrectiveAction policy read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("incident-investigation-corrective-action")]
    [ProducesResponseType(typeof(ApiResponse<IncidentInvestigationCorrectiveActionQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetIncidentInvestigationCorrectiveActionAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetIncidentInvestigationCorrectiveActionQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns MachineMaintenanceInspection policy read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("machine-maintenance-inspection")]
    [ProducesResponseType(typeof(ApiResponse<MachineMaintenanceInspectionQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMachineMaintenanceInspectionAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetMachineMaintenanceInspectionQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns MaterialHandlingStorage policy read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("material-handling-storage")]
    [ProducesResponseType(typeof(ApiResponse<MaterialHandlingStorageQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMaterialHandlingStorageAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetMaterialHandlingStorageQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns PersonalProtectiveEquipment policy read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("personal-protective-equipment")]
    [ProducesResponseType(typeof(ApiResponse<PersonalProtectiveEquipmentQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPersonalProtectiveEquipmentAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetPersonalProtectiveEquipmentQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns SafetyCultureBehavioral policy read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("safety-culture-behavioral")]
    [ProducesResponseType(typeof(ApiResponse<SafetyCultureBehavioralQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSafetyCultureBehavioralAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetSafetyCultureBehavioralQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns SafetyTrainingReporting policy read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("safety-training-reporting")]
    [ProducesResponseType(typeof(ApiResponse<SafetyTrainingReportingQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSafetyTrainingReportingAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetSafetyTrainingReportingQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns SecurityAccessControl policy read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("security-access-control")]
    [ProducesResponseType(typeof(ApiResponse<SecurityAccessControlQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSecurityAccessControlAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetSecurityAccessControlQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns WorkingAtHeights policy read-model data for the caller's tenant.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("working-at-heights")]
    [ProducesResponseType(typeof(ApiResponse<WorkingAtHeightsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetWorkingAtHeightsAsync([FromQuery] CategoryQueryRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();

        var query = new GetWorkingAtHeightsQuery(env.TenantId, request.SiteId, request.From, request.To);

        var result = await _sender.Send(query, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }
}
