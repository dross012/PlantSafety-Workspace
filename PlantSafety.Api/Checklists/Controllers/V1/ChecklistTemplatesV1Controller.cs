// ============================================================================
// File: ChecklistTemplatesV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-21
// Description: V1 controller exposing the Checklists module's ChecklistTemplate commands and its
//              eight sub-component analytics queries (ConfinedSpaceEntry, ContractorSafety,
//              EmergencySystemsAndLifeSafety, HazardousMaterialsAndChemicalSafety, HotWork,
//              MachineGuardingAndLoto, PersonalProtectiveEquipment, WorkingAtHeights) over HTTP.
// ============================================================================
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers(). Routes follow
//   /api/v1/checklists/templates/...
//
// ============================================================================

using PlantSafety.Api.Checklists.Contracts.Requests;
using PlantSafety.Api.Context;
using PlantSafety.Application.Checklists.ChecklistTemplate.ConfinedSpaceEntry.Queries;
using PlantSafety.Application.Checklists.ChecklistTemplate.ContractorSafety.Queries;
using PlantSafety.Application.Checklists.ChecklistTemplate.EmergencySystemsAndLifeSafety.Queries;
using PlantSafety.Application.Checklists.ChecklistTemplate.HazardousMaterialsAndChemicalSafety.Queries;
using PlantSafety.Application.Checklists.ChecklistTemplate.HotWork.Queries;
using PlantSafety.Application.Checklists.ChecklistTemplate.MachineGuardingAndLoto.Queries;
using PlantSafety.Application.Checklists.ChecklistTemplate.PersonalProtectiveEquipment.Queries;
using PlantSafety.Application.Checklists.ChecklistTemplate.WorkingAtHeights.Queries;
using PlantSafety.Application.Checklists.Commands;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Checklists.Controllers.V1;

/// <summary>V1 controller for Checklist Template commands and analytics queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/checklists/templates")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Checklists")]
public sealed class ChecklistTemplatesV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="ChecklistTemplatesV1Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    /// <param name="envelopeFactory">Builds the canonical command/query envelope fields.</param>
    public ChecklistTemplatesV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Creates a new ChecklistTemplate.</summary>
    /// <response code="200">Checklist template created successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateChecklistTemplateResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateChecklistTemplateResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CreateChecklistTemplateResult>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateChecklistTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new CreateChecklistTemplateCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            TenantId: env.TenantId,
            SiteId: request.SiteId,
            OwnerId: env.ActorId,
            TemplateTitle: request.TemplateTitle,
            Category: request.Category,
            Description: request.Description,
            ItemCount: request.ItemCount,
            IsMandatory: request.IsMandatory,
            RegulatoryReference: request.RegulatoryReference);

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Adds an item to a ChecklistTemplate.</summary>
    /// <response code="200">Item added successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Checklist template not found.</response>
    [HttpPost("{checklistTemplateId:guid}/items")]
    [ProducesResponseType(typeof(ApiResponse<AddChecklistTemplateItemResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AddChecklistTemplateItemResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AddChecklistTemplateItemResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddItemAsync(
        Guid checklistTemplateId,
        [FromBody] AddChecklistTemplateItemRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new AddChecklistTemplateItemCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            OwnerId: env.ActorId,
            ChecklistTemplateId: checklistTemplateId,
            Description: request.Description,
            IsCritical: request.IsCritical,
            RegulatoryReference: request.RegulatoryReference)
        {
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Publishes a ChecklistTemplate, making it available for completions.</summary>
    /// <response code="200">Template published successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Checklist template not found.</response>
    [HttpPost("{checklistTemplateId:guid}/publish")]
    [ProducesResponseType(typeof(ApiResponse<PublishChecklistTemplateResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PublishChecklistTemplateResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PublishChecklistTemplateResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PublishAsync(
        Guid checklistTemplateId,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new PublishChecklistTemplateCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            OwnerId: env.ActorId,
            ChecklistTemplateId: checklistTemplateId)
        {
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Retires a ChecklistTemplate.</summary>
    /// <response code="200">Template retired successfully.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    /// <response code="404">Checklist template not found.</response>
    [HttpPost("{checklistTemplateId:guid}/retire")]
    [ProducesResponseType(typeof(ApiResponse<RetireChecklistTemplateResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RetireChecklistTemplateResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RetireChecklistTemplateResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RetireAsync(
        Guid checklistTemplateId,
        [FromBody] RetireChecklistTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = new RetireChecklistTemplateCommand(
            CommandId: env.CommandId,
            CorrelationId: env.CorrelationId,
            CausationId: env.CausationId ?? Guid.NewGuid(),
            RequestedAtUtc: env.RequestedAtUtc,
            ApiVersionToken: env.ApiVersionToken,
            OwnerId: env.ActorId,
            ChecklistTemplateId: checklistTemplateId,
            Reason: request.Reason)
        {
            TenantId = env.TenantId
        };

        var result = await _sender.Send(command, cancellationToken);

        return this.ToActionResult(result, env.CorrelationId);
    }

    // ── Sub-component analytics queries ─────────────────────────────────────

    /// <summary>Returns Confined Space Entry checklist analytics in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/confined-space-entry")]
    [ProducesResponseType(typeof(ApiResponse<ConfinedSpaceEntryQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetConfinedSpaceEntryAsync(
        [FromQuery] ChecklistsAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetConfinedSpaceEntryQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Contractor Safety checklist analytics in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/contractor-safety")]
    [ProducesResponseType(typeof(ApiResponse<ContractorSafetyQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetContractorSafetyAsync(
        [FromQuery] ChecklistsAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetContractorSafetyQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Emergency Systems and Life Safety checklist analytics in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/emergency-systems-and-life-safety")]
    [ProducesResponseType(typeof(ApiResponse<EmergencySystemsAndLifeSafetyQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEmergencySystemsAndLifeSafetyAsync(
        [FromQuery] ChecklistsAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetEmergencySystemsAndLifeSafetyQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Hazardous Materials and Chemical Safety checklist analytics in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/hazardous-materials-and-chemical-safety")]
    [ProducesResponseType(typeof(ApiResponse<HazardousMaterialsAndChemicalSafetyQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHazardousMaterialsAndChemicalSafetyAsync(
        [FromQuery] ChecklistsAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetHazardousMaterialsAndChemicalSafetyQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Hot Work checklist analytics in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/hot-work")]
    [ProducesResponseType(typeof(ApiResponse<HotWorkQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHotWorkAsync(
        [FromQuery] ChecklistsAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetHotWorkQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Machine Guarding and LOTO checklist analytics in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/machine-guarding-and-loto")]
    [ProducesResponseType(typeof(ApiResponse<MachineGuardingAndLotoQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMachineGuardingAndLotoAsync(
        [FromQuery] ChecklistsAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetMachineGuardingAndLotoQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Personal Protective Equipment checklist analytics in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/personal-protective-equipment")]
    [ProducesResponseType(typeof(ApiResponse<PersonalProtectiveEquipmentQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPersonalProtectiveEquipmentAsync(
        [FromQuery] ChecklistsAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetPersonalProtectiveEquipmentQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns Working at Heights checklist analytics in a date range.</summary>
    /// <response code="200">Data returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    /// <response code="403">Caller does not hold the required policy.</response>
    [HttpGet("analytics/working-at-heights")]
    [ProducesResponseType(typeof(ApiResponse<WorkingAtHeightsQueryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetWorkingAtHeightsAsync(
        [FromQuery] ChecklistsAnalyticsQueryRequest request,
        CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetWorkingAtHeightsQuery(env.TenantId, request.SiteId, request.From, request.To);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
}
