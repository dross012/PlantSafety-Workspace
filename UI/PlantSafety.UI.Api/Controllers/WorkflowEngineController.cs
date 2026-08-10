// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           WorkflowEngineController.cs
//  Created:        2026-08-09
//  Description:    "Run this engine" surface over Shared.WorkflowEngine's
//                  pure deterministic global::Shared.WorkflowEngine.
//                  Abstractions.Engine.WorkflowEngine.Evaluate (Phase C.7).
//                  WorkflowContext/WorkflowDecision are themselves plain,
//                  already-public message contracts (not domain entities
//                  with behavior), so they are used directly as the request/
//                  response DTOs here rather than hand-mirrored -- see
//                  Section 13's "UI DTOs only" rule; these Abstractions
//                  records are exactly that shape already.
//
//                  Phase 8: Evaluate now goes through WorkflowEngineTelemetry
//                  instead of calling WorkflowEngine.Evaluate directly --
//                  the decision itself is unchanged, but every invocation
//                  now emits a WORKFLOW_DECISION_EVALUATED structured log
//                  event and a WorkflowDecisionsEvaluated metric. This is
//                  the one real, live execution path Phase 8 instruments
//                  (see WorkflowEngineTelemetry.cs's own header comment).
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;
using Shared.WorkflowEngine.Abstractions.Engine;
using Shared.WorkflowEngine.Infrastructure.Telemetry;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Runs Shared.WorkflowEngine's deterministic per-event evaluation.</summary>
[ApiController]
[Route("api/v1/ui/engines/workflow")]
public sealed class WorkflowEngineController(IClock clock, ILogEventWriter logEventWriter, ICorrelationContext correlationContext) : ControllerBase
{
    /// <summary>POST api/v1/ui/engines/workflow/evaluate -- evaluates a WorkflowContext into a WorkflowDecision.</summary>
    [HttpPost("evaluate")]
    public async Task<ActionResult<WorkflowDecision>> Evaluate([FromBody] WorkflowContext context) =>
        Ok(await WorkflowEngineTelemetry.EvaluateWithTelemetryAsync(context, clock, logEventWriter, correlationContext.CorrelationId));
}
