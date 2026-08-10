// ============================================================================
//  Project:        Shared.WorkflowEngine.Infrastructure
//  Module:         Authorization
//  File:           WorkflowEngineAuthorization.cs
//  Created:        2026-08-10
//  Description:    Phase 9 (Authorization Modernization) deliverable. Wraps
//                  WorkflowEngineTelemetry.EvaluateWithTelemetryAsync (Phase
//                  8, unchanged) with a deterministic authorization check
//                  (Shared.Authorization's real, existing
//                  EnterprisePermissionResolver/AuthorizationCheck) before
//                  invoking it. WorkflowEngine.Evaluate is a read-only query
//                  (no state changes anywhere in this program's WorkflowEngine
//                  -- verified across Phases 4 and 7), so it requires
//                  ModulePermissionVerb.Read, the lowest tier -- not an
//                  invented permission, the correct existing verb for a
//                  read-only operation per ModulePermissionVerb's own
//                  documented mapping.
//
//                  On deny, throws UnauthorizedAccessException -- deliberately,
//                  not a new exception type: Shared.ErrorHandling's existing
//                  ExceptionClassifier (Phase 2.3) already maps
//                  UnauthorizedAccessException to ErrorCategory.Unauthorized,
//                  and ErrorCategoryHttpMapping already maps that to HTTP 401
//                  -- reusing this means a denied request gets a correctly
//                  classified 401 through the existing GlobalErrorHandler
//                  pipeline with zero new error-handling code.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Checks;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Infrastructure.Governance;
using Shared.Authorization.Infrastructure.Telemetry;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.WorkflowEngine.Abstractions.Engine;

namespace Shared.WorkflowEngine.Infrastructure.Authorization;

/// <summary>Enforces a deterministic authorization check before WorkflowEngine.Evaluate.</summary>
public static class WorkflowEngineAuthorization
{
    private const string Module = "WorkflowEngine";

    /// <summary>
    /// Authorizes <paramref name="caller"/> for "WorkflowEngine.Read", then evaluates
    /// <paramref name="context"/> via WorkflowEngineTelemetry.EvaluateWithTelemetryAsync.
    /// Throws UnauthorizedAccessException if the caller's role does not grant "WorkflowEngine.Read".
    /// </summary>
    public static async Task<WorkflowDecision> EvaluateWithAuthorizationAsync(
        WorkflowContext context,
        CallerContext caller,
        IPermissionResolver permissionResolver,
        IClock clock,
        ILogEventWriter logEventWriter)
    {
        AuthorizationGovernanceEvaluator.EvaluateAndReport(caller, Module, clock, logEventWriter);

        AuthorizationDecision decision = AuthorizationCheck.Evaluate(caller, permissionResolver, Module, ModulePermissionVerb.Read);
        AuthorizationTelemetry.RecordDecision(decision, caller, Module, ModulePermissionVerb.Read.ToString(), clock, logEventWriter);

        if (!decision.IsAuthorized)
        {
            throw new UnauthorizedAccessException(decision.Reason);
        }

        return await Telemetry.WorkflowEngineTelemetry.EvaluateWithTelemetryAsync(context, clock, logEventWriter, caller.CorrelationId);
    }
}
