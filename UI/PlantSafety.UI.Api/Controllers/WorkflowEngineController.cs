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
//                  before calling WorkflowEngineTelemetry.
//
//                  Phase 10: the caller is no longer constructed inline here.
//                  ApiKeyAuthenticationMiddleware resolves it once per request
//                  (from the "X-Api-Key" header, or CallerContext.SystemDefault
//                  when absent/unrecognized -- see that middleware's own
//                  header comment) and this controller reads it from
//                  ICallerContextProvider. PlantSafety.UI.Api still has no
//                  real identity provider, so with no API key configured
//                  today the resolved caller is still CallerContext.SystemDefault
//                  -- EnterpriseRole.PlatformAdmin, which the permission model
//                  grants every verb -- so this request's observable behavior
//                  remains unchanged. See CallerContext.cs's own header
//                  comment for why PlatformAdmin-by-default is the honest,
//                  documented placeholder rather than a route around
//                  authorization.
//
//                  Phase 11: Evaluate now goes through WorkflowEngineTenant,
//                  which runs a tenant-isolation check (TenantIsolationCheck)
//                  before WorkflowEngineAuthorization. The resolved
//                  TenantContext comes from the same middleware, via
//                  ITenantContextProvider. requestedFacilityId is passed as
//                  null -- WorkflowContext carries no facility field today
//                  (confirmed before this phase), so there is nothing to
//                  check a boundary against yet; see WorkflowEngineTenant.cs's
//                  own header comment. With no tenant key configured, the
//                  resolved TenantContext is TenantContext.Unscoped, which
//                  TenantIsolationCheck always allows, so this request's
//                  observable behavior remains unchanged.
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.WorkflowEngine.Abstractions.Engine;
using Shared.WorkflowEngine.Infrastructure.Tenant;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Runs Shared.WorkflowEngine's deterministic per-event evaluation.</summary>
[ApiController]
[Route("api/v1/ui/engines/workflow")]
public sealed class WorkflowEngineController(
    IClock clock,
    ILogEventWriter logEventWriter,
    ICallerContextProvider callerContextProvider,
    ITenantContextProvider tenantContextProvider,
    IPermissionResolver permissionResolver,
    ITenantFacilityAccessResolver tenantFacilityAccessResolver) : ControllerBase
{
    /// <summary>POST api/v1/ui/engines/workflow/evaluate -- evaluates a WorkflowContext into a WorkflowDecision.</summary>
    [HttpPost("evaluate")]
    public async Task<ActionResult<WorkflowDecision>> Evaluate([FromBody] WorkflowContext context)
    {
        CallerContext caller = callerContextProvider.Current;
        TenantContext tenantContext = tenantContextProvider.Current;
        return Ok(await WorkflowEngineTenant.EvaluateWithTenantEnforcementAsync(
            context, caller, tenantContext, requestedFacilityId: null, permissionResolver, tenantFacilityAccessResolver, clock, logEventWriter));
    }
}
