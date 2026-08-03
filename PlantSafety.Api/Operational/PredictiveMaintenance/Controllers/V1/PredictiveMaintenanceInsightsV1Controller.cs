// ============================================================================
// File: PredictiveMaintenanceInsightsV1Controller.cs
// Project: PlantSafety.Api
// Module: Operational.PredictiveMaintenance
// Created: 2026-08-03
// Description: V1 controller exposing the 13 "Predictive Maintenance Insights" sub-resources
//              (risk-summary, equipment-health, predictions list/detail/explanation/forecast/
//              recommendations, equipment predictive view, sensor data + anomalies, predictive
//              reports) that PlantSafety.UI's PredictiveMaintenanceApiClient expects. Extends the
//              real Operational.PredictiveMaintenance module rather than building a second
//              "predictive maintenance" domain -- see docs/Phase5/ui-gaps-scoping.md §4. A
//              sibling of PredictiveMaintenanceV1Controller (bare GET, date-range list) and
//              PredictiveMaintenanceAlertV1Controller ("/alerts" sub-route, lifecycle commands +
//              by-id query); this controller owns the remaining sub-paths under the same
//              "api/v{version}/operational/predictive-maintenance" prefix, so none of the three
//              controllers collide on route + verb.
//
// Onboarding Notes:
// - Registered automatically via builder.Services.AddControllers().
// - Every action dispatches through MediatR ISender to the Application layer; no domain logic here.
// - Recommendation detail (GetRecommendationByIdAsync) intentionally reuses
//   GetPredictiveMaintenanceAlertByIdQuery: PredictiveMaintenanceRecommendationDto is modeled 1:1
//   with the alert (RecommendationId == PredictiveMaintenanceAlertId), so recommendation-by-id is
//   an honest re-read of the same alert record rather than a second, uninvertible lookup.
// ============================================================================

using PlantSafety.Api.Common;
using PlantSafety.Api.Context;
using PlantSafety.Api.Operational.PredictiveMaintenance.Authorization;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;
using PlantSafety.Application.Operational.PredictiveMaintenance.Queries;
using PlantSafety.Application.Operational.PredictiveMaintenance.Services;

namespace PlantSafety.Api.Operational.PredictiveMaintenance.Controllers.V1;

/// <summary>V1 controller for the Operational.PredictiveMaintenance module's dashboard/explorer/reporting sub-resources.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/operational/predictive-maintenance")]
[Authorize(Policy = PredictiveMaintenanceAuthorizationPolicies.Read)]
[Tags("Operational.PredictiveMaintenance")]
public sealed class PredictiveMaintenanceInsightsV1Controller : ControllerBase
{
    private readonly ISender _sender;
    private readonly RequestEnvelopeFactory _envelopeFactory;

    /// <summary>Initialises a new instance of <see cref="PredictiveMaintenanceInsightsV1Controller"/>.</summary>
    public PredictiveMaintenanceInsightsV1Controller(ISender sender, RequestEnvelopeFactory envelopeFactory)
    {
        _sender = sender;
        _envelopeFactory = envelopeFactory;
    }

    /// <summary>Returns the Risk Summary card counts.</summary>
    [HttpGet("risk-summary")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveMaintenanceRiskSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRiskSummaryAsync(CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetPredictiveMaintenanceRiskSummaryQuery(env.TenantId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the Equipment Health card percentages.</summary>
    [HttpGet("equipment-health")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveMaintenanceEquipmentHealthDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEquipmentHealthAsync(CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetPredictiveMaintenanceEquipmentHealthQuery(env.TenantId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns a paginated page of predictions, optionally restricted to active (non-terminal) alerts.</summary>
    [HttpGet("predictions")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<PredictiveMaintenancePredictionRowDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPredictionsAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] bool activeOnly = false,
        CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetPredictiveMaintenancePredictionsQuery(env.TenantId, pageNumber, pageSize, activeOnly), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the predictive view header for one equipment asset.</summary>
    [HttpGet("equipment/{equipmentId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveMaintenanceEquipmentViewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEquipmentPredictiveViewAsync(Guid equipmentId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetPredictiveMaintenanceEquipmentViewQuery(env.TenantId, equipmentId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the prediction summary header, sourced directly from the underlying PredictiveMaintenanceAlert record.</summary>
    [HttpGet("predictions/{predictionId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveMaintenancePredictionSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPredictionSummaryAsync(Guid predictionId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var alertResult = await _sender.Send(new GetPredictiveMaintenanceAlertByIdQuery(env.TenantId, predictionId), cancellationToken);

        if (alertResult.IsFailure)
            return this.ToActionResult(PlantSafety.Application.Common.Results.Result<PredictiveMaintenancePredictionSummaryDto>.Failure(alertResult.Error), env.CorrelationId);

        var alert = alertResult.Value;
        var calculator = new PredictiveMaintenanceInsightsCalculator();
        var prediction = calculator.PredictForAlert(alert.Status, alert.PredictedAtUtc, DateTime.UtcNow);
        var updatedAt = alert.ClosedAtUtc ?? alert.DismissedAtUtc ?? alert.ResolvedAtUtc
            ?? alert.ActionScheduledAtUtc ?? alert.ReviewedAtUtc ?? alert.PredictedAtUtc;
        var elapsed = DateTime.UtcNow - updatedAt;

        var summary = new PredictiveMaintenancePredictionSummaryDto
        {
            PredictionId = alert.PredictiveMaintenanceAlertId,
            PredictionCode = PredictiveMaintenanceInsightsCalculator.ShortCode("P", alert.PredictiveMaintenanceAlertId),
            EquipmentId = alert.EquipmentId,
            EquipmentCode = PredictiveMaintenanceInsightsCalculator.ShortCode("EQ", alert.EquipmentId),
            RiskScore = prediction.Output.FailureLikelihood,
            ConfidencePercent = Math.Round(prediction.Confidence.Value * 100.0, 1),
            UpdatedAtUtc = updatedAt,
            DataFreshness = $"Data current as of {FormatElapsed(elapsed)} ago",
        };

        return this.ToActionResult(PlantSafety.Application.Common.Results.Result<PredictiveMaintenancePredictionSummaryDto>.Success(summary), env.CorrelationId);
    }

    /// <summary>Returns the model explanation (feature importance + version) for a prediction.</summary>
    [HttpGet("predictions/{predictionId:guid}/explanation")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveMaintenanceModelExplanationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetModelExplanationAsync(Guid predictionId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetPredictiveMaintenanceModelExplanationQuery(env.TenantId, predictionId), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the risk forecast curve for a prediction.</summary>
    [HttpGet("predictions/{predictionId:guid}/forecast")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<PredictiveMaintenanceForecastPointDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetForecastAsync(
        Guid predictionId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetPredictiveMaintenanceForecastQuery(env.TenantId, predictionId, pageNumber, pageSize), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the recommended actions for a prediction.</summary>
    [HttpGet("predictions/{predictionId:guid}/recommendations")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<PredictiveMaintenanceRecommendationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecommendationsAsync(
        Guid predictionId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetPredictiveMaintenanceRecommendationsQuery(env.TenantId, predictionId, pageNumber, pageSize), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>
    /// Returns the full detail for a single recommendation. Reuses GetPredictiveMaintenanceAlertByIdQuery:
    /// recommendationId is the underlying PredictiveMaintenanceAlertId (see class remarks).
    /// </summary>
    [HttpGet("recommendations/{recommendationId:guid}")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PredictiveMaintenanceRecommendationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecommendationByIdAsync(Guid recommendationId, CancellationToken cancellationToken)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var alertResult = await _sender.Send(new GetPredictiveMaintenanceAlertByIdQuery(env.TenantId, recommendationId), cancellationToken);

        if (alertResult.IsFailure)
            return this.ToActionResult(PlantSafety.Application.Common.Results.Result<PredictiveMaintenanceRecommendationDto>.Failure(alertResult.Error), env.CorrelationId);

        var recommendation = PredictiveMaintenanceRecommendationFactory.Build(alertResult.Value);
        return this.ToActionResult(PlantSafety.Application.Common.Results.Result<PredictiveMaintenanceRecommendationDto>.Success(recommendation), env.CorrelationId);
    }

    /// <summary>Returns sensor time-series data for the Sensor Data Explorer.</summary>
    [HttpGet("sensors/data")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<PredictiveMaintenanceSensorDataPointDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSensorDataAsync(
        [FromQuery] Guid? equipmentId,
        [FromQuery] string? channel,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetPredictiveMaintenanceSensorDataQuery(env.TenantId, equipmentId, channel, pageNumber, pageSize), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns detected anomalies for the Sensor Data Explorer.</summary>
    [HttpGet("sensors/anomalies")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<PredictiveMaintenanceSensorAnomalyDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSensorAnomaliesAsync(
        [FromQuery] Guid? equipmentId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetPredictiveMaintenanceSensorAnomaliesQuery(env.TenantId, equipmentId, pageNumber, pageSize), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    /// <summary>Returns the exportable Predictive Reports list.</summary>
    [HttpGet("reports")]
    [Authorize(Policy = SafetyTierPolicies.Read)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<PredictiveMaintenanceReportDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReportsAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var env = _envelopeFactory.NewQueryEnvelope();
        var result = await _sender.Send(new GetPredictiveMaintenanceReportsQuery(env.TenantId, pageNumber, pageSize), cancellationToken);
        return this.ToActionResult(result, env.CorrelationId);
    }

    private static string FormatElapsed(TimeSpan elapsed)
    {
        if (elapsed.TotalMinutes < 1) return "less than a minute";
        if (elapsed.TotalHours < 1) return $"{(int)elapsed.TotalMinutes} minute(s)";
        if (elapsed.TotalDays < 1) return $"{(int)elapsed.TotalHours} hour(s)";
        return $"{(int)elapsed.TotalDays} day(s)";
    }
}
