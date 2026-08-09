// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           PredictiveModelsController.cs
//  Created:        2026-08-09
//  Description:    "Run this engine" surface over Shared.PredictiveModels'
//                  five deterministic (moving-average/OLS-trend/z-score,
//                  no ML) prediction models (Phase C.11).
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Shared.PredictiveModels.Abstractions.Models;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Request body for the hazard/machine-failure/employee-injury per-period-count models.</summary>
public sealed record PeriodCountSeriesRequest(IReadOnlyList<int> PeriodCounts, int WindowSize);

/// <summary>Request body for the severity-trend model.</summary>
public sealed record SeverityWeightSeriesRequest(IReadOnlyList<int> SeverityWeights, int WindowSize);

/// <summary>Request body for the exposure-anomaly model.</summary>
public sealed record ExposureSeriesRequest(IReadOnlyList<int> PriorPeriodExposureCounts, int LatestPeriodExposureCount, int WindowSize);

/// <summary>Runs Shared.PredictiveModels' five deterministic prediction models.</summary>
[ApiController]
[Route("api/v1/ui/engines/predictive-models")]
public sealed class PredictiveModelsController : ControllerBase
{
    /// <summary>POST .../hazard-likelihood.</summary>
    [HttpPost("hazard-likelihood")]
    public ActionResult<HazardLikelihoodPrediction> HazardLikelihood([FromBody] PeriodCountSeriesRequest request) =>
        Ok(HazardLikelihoodModel.Predict(request.PeriodCounts, request.WindowSize));

    /// <summary>POST .../severity-likelihood.</summary>
    [HttpPost("severity-likelihood")]
    public ActionResult<SeverityLikelihoodPrediction> SeverityLikelihood([FromBody] SeverityWeightSeriesRequest request) =>
        Ok(SeverityLikelihoodModel.Predict(request.SeverityWeights, request.WindowSize));

    /// <summary>POST .../exposure-likelihood.</summary>
    [HttpPost("exposure-likelihood")]
    public ActionResult<ExposureLikelihoodPrediction> ExposureLikelihood([FromBody] ExposureSeriesRequest request) =>
        Ok(ExposureLikelihoodModel.Predict(request.PriorPeriodExposureCounts, request.LatestPeriodExposureCount, request.WindowSize));

    /// <summary>POST .../machine-failure.</summary>
    [HttpPost("machine-failure")]
    public ActionResult<MachineFailurePrediction> MachineFailure([FromBody] PeriodCountSeriesRequest request) =>
        Ok(MachineFailurePredictionModel.Predict(request.PeriodCounts, request.WindowSize));

    /// <summary>POST .../employee-injury.</summary>
    [HttpPost("employee-injury")]
    public ActionResult<EmployeeInjuryPrediction> EmployeeInjury([FromBody] PeriodCountSeriesRequest request) =>
        Ok(EmployeeInjuryPredictionModel.Predict(request.PeriodCounts, request.WindowSize));
}
