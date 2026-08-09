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
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Shared.WorkflowEngine.Abstractions.Engine;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Runs Shared.WorkflowEngine's deterministic per-event evaluation.</summary>
[ApiController]
[Route("api/v1/ui/engines/workflow")]
public sealed class WorkflowEngineController : ControllerBase
{
    /// <summary>POST api/v1/ui/engines/workflow/evaluate -- evaluates a WorkflowContext into a WorkflowDecision.</summary>
    [HttpPost("evaluate")]
    public ActionResult<WorkflowDecision> Evaluate([FromBody] WorkflowContext context) =>
        Ok(global::Shared.WorkflowEngine.Abstractions.Engine.WorkflowEngine.Evaluate(context));
}
