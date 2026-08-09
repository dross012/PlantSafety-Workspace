// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           RulesEngineController.cs
//  Created:        2026-08-09
//  Description:    "Run this engine" surface over Enterprise.RulesEngine's
//                  pure deterministic EnterpriseRulesEngine.Evaluate (Phase
//                  D.5). EnterpriseRule/EnterpriseRuleCondition/
//                  EnterpriseRuleAction are fully data-driven (no delegates),
//                  so the UI can define and submit whole rule sets directly.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Enterprise.RulesEngine.Abstractions.Engine;
using Enterprise.RulesEngine.Abstractions.Rules;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Request body for evaluating a set of enterprise rules against a fact set.</summary>
public sealed record RulesEvaluationRequest(IReadOnlyList<EnterpriseRule> Rules, IReadOnlyDictionary<string, string> Facts);

/// <summary>Runs Enterprise.RulesEngine's deterministic rule evaluation.</summary>
[ApiController]
[Route("api/v1/ui/engines/rules")]
public sealed class RulesEngineController : ControllerBase
{
    /// <summary>POST api/v1/ui/engines/rules/evaluate -- evaluates every supplied rule against the supplied facts.</summary>
    [HttpPost("evaluate")]
    public ActionResult<IReadOnlyList<EnterpriseRuleEvaluationResult>> Evaluate([FromBody] RulesEvaluationRequest request) =>
        Ok(EnterpriseRulesEngine.Evaluate(request.Rules, request.Facts));
}
