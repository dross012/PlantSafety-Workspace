// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           WorkflowAutomationController.cs
//  Created:        2026-08-09
//  Description:    "Run this engine" surface over AI.WorkflowAutomation's
//                  pure deterministic WorkflowAutomationEngine.Automate
//                  (Phase E.3). No shutdown/restart rule catalog exists yet
//                  in Abstractions (typed Func&lt;,&gt; predicates, not
//                  JSON-constructible) -- evaluates with an empty rule set,
//                  which still returns the real Shared.WorkflowEngine-derived
//                  actions/triggers/execution plan. Flagged as a known
//                  scope-out in the final report.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using AI.WorkflowAutomation.Abstractions.Automation;
using AI.WorkflowAutomation.Abstractions.Engine;
using AI.WorkflowAutomation.Abstractions.Rules;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Runs AI.WorkflowAutomation's deterministic workflow-artifact generator.</summary>
[ApiController]
[Route("api/v1/ui/engines/workflow-automation")]
public sealed class WorkflowAutomationController : ControllerBase
{
    /// <summary>POST api/v1/ui/engines/workflow-automation/automate -- generates workflow artifacts (no custom shutdown/restart rule set yet).</summary>
    [HttpPost("automate")]
    public ActionResult<WorkflowAutomationOutput> Automate([FromBody] WorkflowAutomationInput input) =>
        Ok(WorkflowAutomationEngine.Automate(input, Array.Empty<WorkflowAutomationRule>()));
}
