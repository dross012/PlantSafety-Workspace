// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           WorkflowAutomationController.cs
//  Created:        2026-08-09
//  Description:    "Run this engine" surface over AI.WorkflowAutomation's
//                  pure deterministic WorkflowAutomationEngine.Automate
//                  (Phase E.3), evaluated against WorkflowAutomationRuleSet.Standard.
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
    /// <summary>POST api/v1/ui/engines/workflow-automation/automate -- generates workflow artifacts against the standard shutdown/restart rule set.</summary>
    [HttpPost("automate")]
    public ActionResult<WorkflowAutomationOutput> Automate([FromBody] WorkflowAutomationInput input) =>
        Ok(WorkflowAutomationEngine.Automate(input, WorkflowAutomationRuleSet.Standard));
}
