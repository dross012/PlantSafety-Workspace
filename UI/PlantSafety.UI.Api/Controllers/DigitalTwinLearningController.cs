// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           DigitalTwinLearningController.cs
//  Created:        2026-08-09
//  Description:    "Run this engine" surface over AI.DigitalTwinLearning's
//                  pure deterministic DigitalTwinLearningEngine.Learn (Phase
//                  E.7), evaluated against DigitalTwinLearningRuleSet.Standard.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using AI.DigitalTwinLearning.Abstractions.Engine;
using AI.DigitalTwinLearning.Abstractions.Learning;
using AI.DigitalTwinLearning.Abstractions.Rules;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Runs AI.DigitalTwinLearning's deterministic pattern-learning session.</summary>
[ApiController]
[Route("api/v1/ui/engines/digital-twin-learning")]
public sealed class DigitalTwinLearningController : ControllerBase
{
    /// <summary>POST api/v1/ui/engines/digital-twin-learning/learn -- learns patterns against the standard recommendation rule set.</summary>
    [HttpPost("learn")]
    public ActionResult<DigitalTwinLearningOutput> Learn([FromBody] DigitalTwinLearningInput input) =>
        Ok(DigitalTwinLearningEngine.Learn(input, DigitalTwinLearningRuleSet.Standard));
}
