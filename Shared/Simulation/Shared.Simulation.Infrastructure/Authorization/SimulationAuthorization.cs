// ============================================================================
//  Project:        Shared.Simulation.Infrastructure
//  Module:         Authorization
//  File:           SimulationAuthorization.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. Wraps SimulationEvaluationPipeline.
//                  EvaluateAsync (this phase, unchanged) with a deterministic
//                  authorization check before invoking it -- exact mirror of
//                  SafetyGraphAuthorization (Phase 16). Scenario replay/
//                  evaluation is a read-only, deterministic computation (no
//                  state changes anywhere in Shared.Simulation), so it
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
using Shared.Simulation.Abstractions.Simulation;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.Simulation.Infrastructure.Authorization;

/// <summary>Enforces a deterministic authorization check before SimulationEvaluationPipeline.EvaluateAsync.</summary>
public static class SimulationAuthorization
{
    private const string Module = "Simulation";

    /// <summary>
    /// Authorizes <paramref name="caller"/> for "Simulation.Read", then evaluates
    /// <paramref name="request"/> via SimulationEvaluationPipeline.EvaluateAsync. Throws
    /// UnauthorizedAccessException if the caller's role does not grant "Simulation.Read".
    /// </summary>
    public static async Task<SimulationEvaluationResultContract> EvaluateWithAuthorizationAsync(
        SimulationEvaluationRequestContract request,
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

        return await Orchestration.SimulationEvaluationPipeline.EvaluateAsync(request, clock, logEventWriter);
    }
}
