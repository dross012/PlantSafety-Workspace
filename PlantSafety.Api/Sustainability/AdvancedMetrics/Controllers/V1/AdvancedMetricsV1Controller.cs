// ============================================================================
// File: AdvancedMetricsV1Controller.cs
// Project: PlantSafety.Api
// Module: Sustainability.AdvancedMetrics
// Created: 2026-08-04
// Description: V1 controller exposing the Application-layer Sustainability.AdvancedMetrics
//              sub-module's Commands and Queries over HTTP: Scope 1/2/3 carbon accounting,
//              energy/water intensity metering, waste diversion tracking, sustainability target
//              setting, and ESG disclosure report generation/lookup.
//
// Route note: this controller's route ("api/v{version}/sustainability/advanced-metrics") sits
// alongside (and is distinct from) the generic Sustainability initiative lifecycle controller at
// "api/v{version}/sustainability/initiatives".
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Envelope fields are populated via the shared RequestEnvelopeFactory. ActorId always comes
//   from the authenticated caller (ICurrentUserService), never from client-supplied input.
// - All mutating actions use SafetyTierPolicies.Write; reads use SafetyTierPolicies.Read.
// ============================================================================

using PlantSafety.Api.Context;
using PlantSafety.Api.Sustainability.AdvancedMetrics.Contracts.Requests;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Commands;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Queries;

using PlantSafety.Api.Common;

namespace PlantSafety.Api.Sustainability.AdvancedMetrics.Controllers.V1;

/// <summary>V1 controller for the Sustainability.AdvancedMetrics sub-module's commands and queries.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sustainability/advanced-metrics")]
[Authorize(Policy = "RequireAuthenticatedUser")]
[Tags("Sustainability.AdvancedMetrics")]
public sealed class AdvancedMetricsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="AdvancedMetricsV1Controller"/>.</summary>
    public AdvancedMetricsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    // ---- Commands ------------------------------------------------------------

    /// <summary>Records a single Scope 1/2/3 carbon emission activity.</summary>
    [HttpPost("carbon-emissions")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RecordCarbonEmissionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RecordCarbonEmissionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordCarbonEmissionAsync([FromBody] RecordCarbonEmissionRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = RecordCarbonEmissionCommand.FromDto(
            tenantId: env.TenantId, siteId: request.SiteId,
            reportingPeriodStartUtc: request.ReportingPeriodStartUtc, reportingPeriodEndUtc: request.ReportingPeriodEndUtc,
            scope: request.Scope, activityDescription: request.ActivityDescription,
            emissionsMetricTonsCo2e: request.EmissionsMetricTonsCo2e, methodologyVersion: request.MethodologyVersion,
            activityAtUtc: request.ActivityAtUtc, requesterUserId: env.ActorId,
            correlationId: env.CorrelationId, causationId: env.CausationId, apiVersionToken: env.ApiVersionToken);

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records an energy-intensity meter read.</summary>
    [HttpPost("energy-intensity")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RecordEnergyIntensityMetricResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RecordEnergyIntensityMetricResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordEnergyIntensityMetricAsync([FromBody] RecordEnergyIntensityMetricRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = RecordEnergyIntensityMetricCommand.FromDto(
            tenantId: env.TenantId, siteId: request.SiteId,
            reportingPeriodStartUtc: request.ReportingPeriodStartUtc, reportingPeriodEndUtc: request.ReportingPeriodEndUtc,
            energyConsumedKwh: request.EnergyConsumedKwh, outputUnits: request.OutputUnits,
            methodologyVersion: request.MethodologyVersion, readingAtUtc: request.ReadingAtUtc,
            requesterUserId: env.ActorId, correlationId: env.CorrelationId, causationId: env.CausationId, apiVersionToken: env.ApiVersionToken);

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a water-intensity meter read.</summary>
    [HttpPost("water-intensity")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RecordWaterIntensityMetricResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RecordWaterIntensityMetricResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordWaterIntensityMetricAsync([FromBody] RecordWaterIntensityMetricRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = RecordWaterIntensityMetricCommand.FromDto(
            tenantId: env.TenantId, siteId: request.SiteId,
            reportingPeriodStartUtc: request.ReportingPeriodStartUtc, reportingPeriodEndUtc: request.ReportingPeriodEndUtc,
            waterConsumedCubicMeters: request.WaterConsumedCubicMeters, outputUnits: request.OutputUnits,
            methodologyVersion: request.MethodologyVersion, readingAtUtc: request.ReadingAtUtc,
            requesterUserId: env.ActorId, correlationId: env.CorrelationId, causationId: env.CausationId, apiVersionToken: env.ApiVersionToken);

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Records a waste diversion interval.</summary>
    [HttpPost("waste-diversion")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<RecordWasteDiversionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RecordWasteDiversionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RecordWasteDiversionAsync([FromBody] RecordWasteDiversionRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = RecordWasteDiversionCommand.FromDto(
            tenantId: env.TenantId, siteId: request.SiteId,
            reportingPeriodStartUtc: request.ReportingPeriodStartUtc, reportingPeriodEndUtc: request.ReportingPeriodEndUtc,
            totalWasteKg: request.TotalWasteKg, divertedWasteKg: request.DivertedWasteKg,
            methodologyVersion: request.MethodologyVersion, intervalEndAtUtc: request.IntervalEndAtUtc,
            requesterUserId: env.ActorId, correlationId: env.CorrelationId, causationId: env.CausationId, apiVersionToken: env.ApiVersionToken);

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Sets (or re-baselines) a sustainability KPI target.</summary>
    [HttpPost("targets")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<SetSustainabilityTargetResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SetSustainabilityTargetResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetSustainabilityTargetAsync([FromBody] SetSustainabilityTargetRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = SetSustainabilityTargetCommand.FromDto(
            tenantId: env.TenantId, siteId: request.SiteId,
            reportingPeriodStartUtc: request.ReportingPeriodStartUtc, reportingPeriodEndUtc: request.ReportingPeriodEndUtc,
            targetType: request.TargetType, baselineValue: request.BaselineValue, targetValue: request.TargetValue,
            targetDateUtc: request.TargetDateUtc, requesterUserId: env.ActorId,
            correlationId: env.CorrelationId, causationId: env.CausationId, apiVersionToken: env.ApiVersionToken);

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Generates a new draft ESG disclosure report from a source AdvancedMetricsLedger.</summary>
    [HttpPost("disclosure-reports")]
    [Authorize(Policy = SafetyTierPolicies.Write)]
    [ProducesResponseType(typeof(ApiResponse<GenerateEsgDisclosureReportResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<GenerateEsgDisclosureReportResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GenerateEsgDisclosureReportAsync([FromBody] GenerateEsgDisclosureReportRequest request, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewCommandEnvelope();

        var command = GenerateEsgDisclosureReportCommand.FromDto(
            tenantId: env.TenantId, siteId: request.SiteId, sourceLedgerId: request.SourceLedgerId,
            frameworks: request.Frameworks, methodologyVersion: request.MethodologyVersion,
            requesterUserId: env.ActorId, correlationId: env.CorrelationId, causationId: env.CausationId, apiVersionToken: env.ApiVersionToken);

        var result = await _sender.Send(command, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    // ---- Queries -------------------------------------------------------------

    /// <summary>Returns the Scope 1/2/3 carbon footprint rollup for a reporting period (Carbon Footprint Trend dashboard).</summary>
    [HttpGet("carbon-footprint")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Sustainability.AdvancedMetrics.Models.CarbonFootprintByPeriodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCarbonFootprintByPeriodAsync(
        [FromQuery] Guid? siteId, [FromQuery] DateTime periodStartUtc, [FromQuery] DateTime periodEndUtc, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetCarbonFootprintByPeriodQuery(env.TenantId, siteId, periodStartUtc, periodEndUtc);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the energy-intensity trend points for a ledger (Energy/Water Intensity Board dashboard).</summary>
    [HttpGet("{advancedMetricsLedgerId:guid}/energy-intensity-trend")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PlantSafety.Application.Sustainability.AdvancedMetrics.Models.EnergyIntensityTrendPointDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEnergyIntensityTrendAsync(Guid advancedMetricsLedgerId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetEnergyIntensityTrendQuery(env.TenantId, advancedMetricsLedgerId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the water-intensity trend points for a ledger (Energy/Water Intensity Board dashboard).</summary>
    [HttpGet("{advancedMetricsLedgerId:guid}/water-intensity-trend")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PlantSafety.Application.Sustainability.AdvancedMetrics.Models.WaterIntensityTrendPointDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetWaterIntensityTrendAsync(Guid advancedMetricsLedgerId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetWaterIntensityTrendQuery(env.TenantId, advancedMetricsLedgerId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the waste diversion rate rollup for a ledger (Waste Diversion Rate dashboard).</summary>
    [HttpGet("{advancedMetricsLedgerId:guid}/waste-diversion-rate")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Sustainability.AdvancedMetrics.Models.WasteDiversionRateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetWasteDiversionRateAsync(Guid advancedMetricsLedgerId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetWasteDiversionRateQuery(env.TenantId, advancedMetricsLedgerId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns all sustainability targets and their tracked progress for a ledger (Target Progress Tracker dashboard).</summary>
    [HttpGet("{advancedMetricsLedgerId:guid}/targets")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PlantSafety.Application.Sustainability.AdvancedMetrics.Models.SustainabilityTargetProgressDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListSustainabilityTargetsAndProgressAsync(Guid advancedMetricsLedgerId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new ListSustainabilityTargetsAndProgressQuery(env.TenantId, advancedMetricsLedgerId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a single ESG disclosure report by identity.</summary>
    [HttpGet("disclosure-reports/{esgDisclosureReportId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PlantSafety.Application.Sustainability.AdvancedMetrics.Models.EsgDisclosureReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEsgDisclosureReportByIdAsync(Guid esgDisclosureReportId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var query = new GetEsgDisclosureReportByIdQuery(env.TenantId, esgDisclosureReportId);
        var result = await _sender.Send(query, cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }
}
