// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           SeverityAssessmentController.cs
//  Created:        2026-08-09
//  Description:    "Run this engine" surface over AI.SeverityAssessment's
//                  pure deterministic SeverityAssessmentEngine.Assess (Phase
//                  E.5). No annotation-rule catalog exists yet in
//                  Abstractions (typed Func&lt;,&gt; predicates, not
//                  JSON-constructible) -- evaluates with an empty rule set,
//                  which still returns the real base/final assessed severity.
//                  Flagged as a known scope-out in the final report.
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
    /// <summary>POST api/v1/ui/engines/severity-assessment/assess -- assesses severity (no custom annotation rule set yet).</summary>
    [HttpPost("assess")]
    public ActionResult<SeverityAssessmentOutput> Assess([FromBody] SeverityAssessmentInput input) =>
        Ok(SeverityAssessmentEngine.Assess(input, Array.Empty<SeverityAssessmentRule>()));
}
