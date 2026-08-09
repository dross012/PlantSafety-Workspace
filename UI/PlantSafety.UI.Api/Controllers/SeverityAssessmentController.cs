// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           SeverityAssessmentController.cs
//  Created:        2026-08-09
//  Description:    "Run this engine" surface over AI.SeverityAssessment's
//                  pure deterministic SeverityAssessmentEngine.Assess (Phase
//                  E.5), evaluated against SeverityAssessmentRuleSet.Standard.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using AI.SeverityAssessment.Abstractions.Assessment;
using AI.SeverityAssessment.Abstractions.Engine;
using AI.SeverityAssessment.Abstractions.Rules;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Runs AI.SeverityAssessment's deterministic severity assessment.</summary>
[ApiController]
[Route("api/v1/ui/engines/severity-assessment")]
public sealed class SeverityAssessmentController : ControllerBase
{
    /// <summary>POST api/v1/ui/engines/severity-assessment/assess -- assesses severity against the standard rule set.</summary>
    [HttpPost("assess")]
    public ActionResult<SeverityAssessmentOutput> Assess([FromBody] SeverityAssessmentInput input) =>
        Ok(SeverityAssessmentEngine.Assess(input, SeverityAssessmentRuleSet.Standard));
}
