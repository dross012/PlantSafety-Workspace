// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           EnterpriseOrchestrationController.cs
//  Created:        2026-08-09
//  Description:    "Run this engine" surface over AI.EnterpriseOrchestration's
//                  pure deterministic EnterpriseOrchestrationEngine.Orchestrate
//                  (Phase E.9). The input (a list of FacilityOrchestrationContext,
//                  each nesting four other engines' full outputs) is accepted
//                  as-is via [FromBody] -- an advanced/expert JSON payload,
//                  not a per-field form, since flattening five nested engine
//                  outputs into individual form fields was judged out of
//                  scope for this pass (documented in the final report). No
//                  alert-rule catalog exists yet either (typed Func&lt;,&gt;
//                  predicates) -- evaluates with an empty rule set.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using AI.EnterpriseOrchestration.Abstractions.Engine;
using AI.EnterpriseOrchestration.Abstractions.Orchestration;
using AI.EnterpriseOrchestration.Abstractions.Rules;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Runs AI.EnterpriseOrchestration's deterministic cross-facility aggregation.</summary>
[ApiController]
[Route("api/v1/ui/engines/enterprise-orchestration")]
public sealed class EnterpriseOrchestrationController : ControllerBase
{
    /// <summary>POST api/v1/ui/engines/enterprise-orchestration/orchestrate -- aggregates across facilities (no custom alert rule set yet).</summary>
    [HttpPost("orchestrate")]
    public ActionResult<EnterpriseOrchestrationOutput> Orchestrate([FromBody] EnterpriseOrchestrationInput input) =>
        Ok(EnterpriseOrchestrationEngine.Orchestrate(input, Array.Empty<EnterpriseOrchestrationRule>()));
}
