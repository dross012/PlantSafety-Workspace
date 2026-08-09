// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           WorkflowOrchestrationController.cs
//  Created:        2026-08-09
//  Description:    "Run this engine" surface over Enterprise.
//                  WorkflowOrchestration's pure deterministic
//                  EnterpriseWorkflowOrchestrator (Phase D.6): execution-plan
//                  ordering and guarded state transitions.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Enterprise.WorkflowOrchestration.Abstractions.Ids;
using Enterprise.WorkflowOrchestration.Abstractions.Orchestrator;
using Enterprise.WorkflowOrchestration.Abstractions.States;
using Enterprise.WorkflowOrchestration.Abstractions.Steps;
using Enterprise.WorkflowOrchestration.Abstractions.Transitions;

using Shared.Deterministic.Abstractions.Clock;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Request body for a guarded workflow state transition.</summary>
public sealed record WorkflowTransitionRequest(Guid WorkflowId, EnterpriseWorkflowState FromState, EnterpriseWorkflowState ToState);

/// <summary>Runs Enterprise.WorkflowOrchestration's deterministic plan-building and transition-guarding.</summary>
[ApiController]
[Route("api/v1/ui/engines/workflow-orchestration")]
public sealed class WorkflowOrchestrationController(IClock clock) : ControllerBase
{
    /// <summary>POST .../build-plan -- orders the given steps into a deterministic execution plan.</summary>
    [HttpPost("build-plan")]
    public ActionResult<IReadOnlyList<EnterpriseWorkflowStep>> BuildPlan([FromBody] IReadOnlyList<EnterpriseWorkflowStep> steps) =>
        Ok(EnterpriseWorkflowOrchestrator.BuildExecutionPlan(steps));

    /// <summary>POST .../transition -- produces a guarded state transition, or 400 if invalid.</summary>
    [HttpPost("transition")]
    public ActionResult<EnterpriseWorkflowTransition> Transition([FromBody] WorkflowTransitionRequest request)
    {
        try
        {
            return Ok(EnterpriseWorkflowOrchestrator.Transition(
                EnterpriseWorkflowId.From(request.WorkflowId), request.FromState, request.ToState, clock.UtcNow));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
