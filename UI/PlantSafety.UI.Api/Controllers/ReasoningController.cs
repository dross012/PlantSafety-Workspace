// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           ReasoningController.cs
//  Created:        2026-08-09
//  Description:    "Run this engine" surface over AI.Reasoning's pure
//                  deterministic SafetyReasoningEngine.Reason (Phase E.1),
//                  evaluated against SafetyReasoningRuleSet.Standard.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using AI.Reasoning.Abstractions.Context;
using AI.Reasoning.Abstractions.Engine;
using AI.Reasoning.Abstractions.Rules;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Runs AI.Reasoning's deterministic safety reasoning session.</summary>
[ApiController]
[Route("api/v1/ui/engines/reasoning")]
public sealed class ReasoningController : ControllerBase
{
    /// <summary>POST api/v1/ui/engines/reasoning/reason -- reasons over the given input against the standard rule set.</summary>
    [HttpPost("reason")]
    public ActionResult<SafetyReasoningOutput> Reason([FromBody] SafetyReasoningInput input)
    {
        var context = new SafetyReasoningContext(Guid.NewGuid(), input, DateTime.UtcNow);
        return Ok(SafetyReasoningEngine.Reason(context, SafetyReasoningRuleSet.Standard));
    }
}
