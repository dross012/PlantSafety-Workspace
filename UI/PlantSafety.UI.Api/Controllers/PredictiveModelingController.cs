// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           PredictiveModelingController.cs
//  Created:        2026-08-09
//  Description:    "Run this engine" surface over Enterprise.
//                  PredictiveModeling's four deterministic enterprise-wide
//                  forecasts (Phase D.8).
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Enterprise.PredictiveModeling.Abstractions.Engine;
using Enterprise.PredictiveModeling.Abstractions.Forecasts;

using Shared.Deterministic.Abstractions.Clock;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Request body for the risk/hazard/severity forecasts (double series).</summary>
public sealed record DoubleSeriesForecastRequest(IReadOnlyList<double> History, int WindowSize);

/// <summary>Request body for the hazard/severity forecasts (int series).</summary>
public sealed record IntSeriesForecastRequest(IReadOnlyList<int> History, int WindowSize);

/// <summary>Runs Enterprise.PredictiveModeling's four deterministic enterprise-wide forecasts.</summary>
[ApiController]
[Route("api/v1/ui/engines/predictive-modeling")]
public sealed class PredictiveModelingController(IClock clock) : ControllerBase
{
    /// <summary>POST .../risk.</summary>
    [HttpPost("risk")]
    public ActionResult<EnterpriseRiskForecast> Risk([FromBody] DoubleSeriesForecastRequest request) =>
        Ok(EnterprisePredictiveModelEngine.ForecastRisk(request.History, request.WindowSize, clock.UtcNow));

    /// <summary>POST .../compliance.</summary>
    [HttpPost("compliance")]
    public ActionResult<EnterpriseComplianceForecast> Compliance([FromBody] DoubleSeriesForecastRequest request) =>
        Ok(EnterprisePredictiveModelEngine.ForecastCompliance(request.History, request.WindowSize, clock.UtcNow));

    /// <summary>POST .../hazard.</summary>
    [HttpPost("hazard")]
    public ActionResult<EnterpriseHazardForecast> Hazard([FromBody] IntSeriesForecastRequest request) =>
        Ok(EnterprisePredictiveModelEngine.ForecastHazard(request.History, request.WindowSize, clock.UtcNow));

    /// <summary>POST .../severity.</summary>
    [HttpPost("severity")]
    public ActionResult<EnterpriseSeverityForecast> Severity([FromBody] IntSeriesForecastRequest request) =>
        Ok(EnterprisePredictiveModelEngine.ForecastSeverity(request.History, request.WindowSize, clock.UtcNow));
}
