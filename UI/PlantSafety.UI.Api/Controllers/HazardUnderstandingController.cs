// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           HazardUnderstandingController.cs
//  Created:        2026-08-09
//  Description:    "Run this engine" surface over AI.HazardUnderstanding's
//                  pure deterministic HazardUnderstandingEngine.Understand
//                  (Phase E.2).
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using AI.HazardUnderstanding.Abstractions.Engine;
using AI.HazardUnderstanding.Abstractions.Semantics;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Runs AI.HazardUnderstanding's deterministic semantic-model builder.</summary>
[ApiController]
[Route("api/v1/ui/engines/hazard-understanding")]
public sealed class HazardUnderstandingController : ControllerBase
{
    /// <summary>POST api/v1/ui/engines/hazard-understanding/understand.</summary>
    [HttpPost("understand")]
    public ActionResult<HazardUnderstandingOutput> Understand([FromBody] HazardUnderstandingInput input) =>
        Ok(HazardUnderstandingEngine.Understand(input));
}
