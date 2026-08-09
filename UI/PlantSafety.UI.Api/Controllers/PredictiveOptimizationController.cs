// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           PredictiveOptimizationController.cs
//  Created:        2026-08-09
//  Description:    "Run this engine" surface over AI.PredictiveOptimization's
//                  pure deterministic PredictiveOptimizationEngine.Optimize
//                  (Phase E.6).
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using AI.PredictiveOptimization.Abstractions.Engine;
using AI.PredictiveOptimization.Abstractions.Optimization;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Runs AI.PredictiveOptimization's deterministic weighted-target optimizer.</summary>
[ApiController]
[Route("api/v1/ui/engines/predictive-optimization")]
public sealed class PredictiveOptimizationController : ControllerBase
{
    /// <summary>POST api/v1/ui/engines/predictive-optimization/optimize.</summary>
    [HttpPost("optimize")]
    public ActionResult<PredictiveOptimizationOutput> Optimize([FromBody] PredictiveOptimizationInput input)
    {
        try
        {
            return Ok(PredictiveOptimizationEngine.Optimize(input));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
