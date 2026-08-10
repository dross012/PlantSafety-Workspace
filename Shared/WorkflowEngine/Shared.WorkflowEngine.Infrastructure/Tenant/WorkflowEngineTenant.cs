// ============================================================================
//  Project:        Shared.WorkflowEngine.Infrastructure
//  Module:         Tenant
//  File:           WorkflowEngineTenant.cs
//  Created:        2026-08-10
//  Description:    Phase 11 (Multi-Tenant & Contextual Access Modernization)
//                  deliverable. Wraps WorkflowEngineAuthorization.
//                  EvaluateWithAuthorizationAsync (Phase 9, unchanged) with a
//                  deterministic tenant-isolation check
//                  (Shared.Authorization's TenantIsolationCheck) before
//                  invoking it.
//
//                  requestedFacilityId is threaded in as an explicit
//                  parameter rather than read off WorkflowContext, because
//                  WorkflowContext (Shared.WorkflowEngine.Abstractions.Engine)
//                  carries no facility/tenant field today -- confirmed before
//                  writing this file. The one real, live call site
//                  (WorkflowEngineController.Evaluate) has no facility-scoped
//                  request data to supply yet, so it passes null, which
//                  TenantIsolationCheck treats as "not facility-scoped; allow"
//                  -- this wrapper is real, tested, and wired, but -- like
//                  Phase 8's FacilityTelemetry and Phase 9's FacilityAuthorization
//                  before it -- its facility-boundary branch is dormant until
//                  a real facility-scoped request shape exists. This is
//                  documented honestly rather than invented around.
//
//                  On deny, throws UnauthorizedAccessException -- same reuse
//                  of Shared.ErrorHandling's ExceptionClassifier -> 401
//                  mapping WorkflowEngineAuthorization.cs's own header
//                  comment explains.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Authorization.Infrastructure.Governance;
using Shared.Authorization.Infrastructure.Telemetry;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.WorkflowEngine.Abstractions.Engine;
using Shared.WorkflowEngine.Infrastructure.Authorization;

namespace Shared.WorkflowEngine.Infrastructure.Tenant;

/// <summary>Enforces a deterministic tenant-isolation check before WorkflowEngineAuthorization.EvaluateWithAuthorizationAsync.</summary>
public static class WorkflowEngineTenant
{
    private const string Module = "WorkflowEngine";

    /// <summary>
    /// Evaluates <paramref name="tenantContext"/>'s access to <paramref name="requestedFacilityId"/>, then
    /// authorizes and evaluates <paramref name="context"/> via WorkflowEngineAuthorization.
    /// Throws UnauthorizedAccessException on tenant-isolation denial or authorization denial.
    /// </summary>
    public static async Task<WorkflowDecision> EvaluateWithTenantEnforcementAsync(
        WorkflowContext context,
        CallerContext caller,
        TenantContext tenantContext,
        Guid? requestedFacilityId,
        IPermissionResolver permissionResolver,
        ITenantFacilityAccessResolver? tenantFacilityAccessResolver,
        IClock clock,
        ILogEventWriter logEventWriter)
    {
        TenantGovernanceEvaluator.EvaluateContextAndReport(tenantContext, clock, logEventWriter);

        TenantAccessDecision decision = TenantIsolationCheck.Evaluate(tenantContext, requestedFacilityId, tenantFacilityAccessResolver);
        TenantTelemetry.RecordIsolationDecision(decision, tenantContext, Module, clock, logEventWriter);
        TenantGovernanceEvaluator.EvaluateIsolationDecisionAndReport(decision, requestedFacilityId, tenantContext.CorrelationId, clock, logEventWriter);

        if (!decision.IsAuthorized)
        {
            throw new UnauthorizedAccessException(decision.Reason);
        }

        return await WorkflowEngineAuthorization.EvaluateWithAuthorizationAsync(context, caller, permissionResolver, clock, logEventWriter);
    }
}
