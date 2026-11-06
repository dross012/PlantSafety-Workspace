// ============================================================================
//  Project:        Shared.SafetyGraph.Infrastructure
//  Module:         Authorization
//  File:           SafetyGraphAuthorization.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. Wraps SafetyGraphEvaluationPipeline.
//                  EvaluateAsync (this phase, unchanged) with a deterministic
//                  authorization check before invoking it -- exact mirror of
//                  SchedulingAuthorization/PredictiveAuthorization (Phases
//                  14/15). Graph construction/traversal/inference is a
//                  read-only, deterministic computation (no state changes
//                  anywhere in Shared.SafetyGraph), so it requires
//                  ModulePermissionVerb.Read, the lowest tier.
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
using Shared.SafetyGraph.Abstractions.Graph;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.SafetyGraph.Infrastructure.Authorization;

/// <summary>Enforces a deterministic authorization check before SafetyGraphEvaluationPipeline.EvaluateAsync.</summary>
public static class SafetyGraphAuthorization
{
    private const string Module = "SafetyGraph";

    /// <summary>
    /// Authorizes <paramref name="caller"/> for "SafetyGraph.Read", then evaluates
    /// <paramref name="request"/> via SafetyGraphEvaluationPipeline.EvaluateAsync. Throws
    /// UnauthorizedAccessException if the caller's role does not grant "SafetyGraph.Read".
    /// </summary>
    public static async Task<SafetyGraphEvaluationResultContract> EvaluateWithAuthorizationAsync(
        SafetyGraphEvaluationRequestContract request,
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

        return await Orchestration.SafetyGraphEvaluationPipeline.EvaluateAsync(request, clock, logEventWriter);
    }
}
