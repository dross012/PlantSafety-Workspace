// ============================================================================
//  Project:        Shared.PredictiveSafety.Infrastructure
//  Module:         Authorization
//  File:           PredictiveAuthorization.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. Wraps PredictiveSafetyEvaluationPipeline.
//                  EvaluateAsync (this phase, unchanged) with a deterministic
//                  authorization check before invoking it -- exact mirror of
//                  SchedulingAuthorization (Phase 14). The full five-domain
//                  evaluation is a read-only, deterministic computation (no
//                  state changes anywhere in Shared.PredictiveSafety), so it
//                  requires ModulePermissionVerb.Read, the lowest tier.
//
//                  On deny, throws UnauthorizedAccessException -- same reuse
//                  of Shared.ErrorHandling's ExceptionClassifier -> 401
//                  mapping every prior phase's *Authorization.cs documents.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Checks;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Infrastructure.Governance;
using Shared.Authorization.Infrastructure.Telemetry;
using Shared.Deterministic.Abstractions.Clock;
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.PredictiveSafety.Infrastructure.Authorization;

/// <summary>Enforces a deterministic authorization check before PredictiveSafetyEvaluationPipeline.EvaluateAsync.</summary>
public static class PredictiveAuthorization
{
    private const string Module = "PredictiveSafety";

    /// <summary>
    /// Authorizes <paramref name="caller"/> for "PredictiveSafety.Read", then evaluates
    /// <paramref name="request"/> via PredictiveSafetyEvaluationPipeline.EvaluateAsync. Throws
    /// UnauthorizedAccessException if the caller's role does not grant "PredictiveSafety.Read".
    /// </summary>
    public static async Task<PredictiveSafetyEvaluationResultContract> EvaluateWithAuthorizationAsync(
        PredictiveSafetyEvaluationRequestContract request,
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

        return await Orchestration.PredictiveSafetyEvaluationPipeline.EvaluateAsync(request, clock, logEventWriter);
    }
}
