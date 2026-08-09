// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           WorkflowOrchestrationController.cs
//  Created:        2026-08-09
//  Description:    "Run this engine" surface over Enterprise.
//                  WorkflowOrchestration's pure deterministic
//                  EnterpriseWorkflowOrchestrator (Phase D.6): execution-plan
//                  ordering and guarded state transitions. Phase 2.8 adds
//                  .../execute, applying Shared.Workflow's deterministic
//                  execution layer (envelope + rollback rules) to a built
//                  plan against caller-supplied per-step outcomes -- this
//                  engine has no real actuation runtime of its own (same
//                  as every other pure/composition engine in this
//                  program), so the caller supplies what each step's
//                  outcome was/would be, and gets back which steps
//                  completed, failed, were rolled back, or skipped.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Enterprise.WorkflowOrchestration.Abstractions.Ids;
using Enterprise.WorkflowOrchestration.Abstractions.Orchestrator;
using Enterprise.WorkflowOrchestration.Abstractions.States;
using Enterprise.WorkflowOrchestration.Abstractions.Steps;
using Enterprise.WorkflowOrchestration.Abstractions.Transitions;

using Shared.Deterministic.Abstractions.Clock;
using Shared.Deterministic.Abstractions.Random;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Workflow.Abstractions.Envelope;
using Shared.Workflow.Abstractions.Execution;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Request body for a guarded workflow state transition.</summary>
public sealed record WorkflowTransitionRequest(Guid WorkflowId, EnterpriseWorkflowState FromState, EnterpriseWorkflowState ToState);

/// <summary>Request body for executing a built plan against caller-supplied per-step outcomes.</summary>
public sealed record WorkflowExecuteRequest(IReadOnlyList<EnterpriseWorkflowStep> Steps, IReadOnlyList<bool> StepOutcomes);

/// <summary>Runs Enterprise.WorkflowOrchestration's deterministic plan-building and transition-guarding.</summary>
[ApiController]
[Route("api/v1/ui/engines/workflow-orchestration")]
public sealed class WorkflowOrchestrationController(IClock clock, IRandomProvider randomProvider, ICorrelationContext correlationContext) : ControllerBase
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

    /// <summary>POST .../execute -- deterministically sequences a plan against caller-supplied per-step outcomes, applying rollback rules on the first failure.</summary>
    [HttpPost("execute")]
    public ActionResult<IReadOnlyList<WorkflowStepEnvelope<EnterpriseWorkflowStep>>> Execute([FromBody] WorkflowExecuteRequest request)
    {
        try
        {
            return Ok(WorkflowExecutionEngine.Execute(
                request.Steps, request.StepOutcomes, randomProvider.NewGuid(), correlationContext.CorrelationId, "WorkflowOrchestration", randomProvider.NewGuid));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
