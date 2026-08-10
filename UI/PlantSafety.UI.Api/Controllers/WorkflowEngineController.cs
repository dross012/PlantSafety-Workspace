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
//                  Phase 8: Evaluate goes through WorkflowEngineTelemetry --
//                  the decision itself is unchanged, but every invocation
//                  emits a WORKFLOW_DECISION_EVALUATED structured log event
//                  and a WorkflowDecisionsEvaluated metric.
//
//                  Phase 9: Evaluate now goes through WorkflowEngineAuthorization,
//                  which runs a real, deterministic permission check
//                  (Shared.Authorization's EnterprisePermissionResolver)
//                  before calling WorkflowEngineTelemetry. PlantSafety.UI.Api
//                  has no authentication scheme, so the caller is
//                  CallerContext.SystemDefault -- EnterpriseRole.PlatformAdmin,
//                  which the permission model grants every verb, so this
//                  request's observable behavior is unchanged today. See
//                  CallerContext.cs's own header comment for why this is the
//                  honest, documented placeholder rather than a route around
//                  authorization.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;
using Shared.WorkflowEngine.Abstractions.Engine;
using Shared.WorkflowEngine.Infrastructure.Authorization;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Runs Shared.WorkflowEngine's deterministic per-event evaluation.</summary>
[ApiController]
[Route("api/v1/ui/engines/workflow")]
public sealed class WorkflowEngineController(
    IClock clock,
    ILogEventWriter logEventWriter,
    ICorrelationContext correlationContext,
    IPermissionResolver permissionResolver) : ControllerBase
{
    /// <summary>POST api/v1/ui/engines/workflow/evaluate -- evaluates a WorkflowContext into a WorkflowDecision.</summary>
    [HttpPost("evaluate")]
    public async Task<ActionResult<WorkflowDecision>> Evaluate([FromBody] WorkflowContext context)
    {
        CallerContext caller = CallerContext.SystemDefault(correlationContext.CorrelationId);
        return Ok(await WorkflowEngineAuthorization.EvaluateWithAuthorizationAsync(context, caller, permissionResolver, clock, logEventWriter));
    }
}
