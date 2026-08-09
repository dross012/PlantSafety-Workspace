// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           RegulatoryInterpretationController.cs
//  Created:        2026-08-09
//  Description:    "Run this engine" surface over AI.RegulatoryInterpretation's
//                  pure deterministic RegulatoryInterpretationEngine.Interpret
//                  (Phase E.4).
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using AI.RegulatoryInterpretation.Abstractions.Engine;
using AI.RegulatoryInterpretation.Abstractions.Interpretation;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Runs AI.RegulatoryInterpretation's deterministic jurisdiction interpretation.</summary>
[ApiController]
[Route("api/v1/ui/engines/regulatory-interpretation")]
public sealed class RegulatoryInterpretationController : ControllerBase
{
    /// <summary>POST api/v1/ui/engines/regulatory-interpretation/interpret.</summary>
    [HttpPost("interpret")]
    public ActionResult<RegulatoryInterpretationOutput> Interpret([FromBody] RegulatoryInterpretationInput input) =>
        Ok(RegulatoryInterpretationEngine.Interpret(input));
}
