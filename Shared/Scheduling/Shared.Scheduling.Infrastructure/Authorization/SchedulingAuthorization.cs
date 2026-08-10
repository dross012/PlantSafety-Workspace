// ============================================================================
//  Project:        Shared.Scheduling.Infrastructure
//  Module:         Authorization
//  File:           SchedulingAuthorization.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable. Wraps
//                  SchedulingTelemetry.CalculateWithTelemetryAsync (this
//                  phase, unchanged) with a deterministic authorization
//                  check (Shared.Authorization's EnterprisePermissionResolver/
//                  AuthorizationCheck) before invoking it -- exact mirror of
//                  WorkflowEngineAuthorization (Phase 9) and RoutingAuthorization
//                  (Phase 13). SchedulingScheduleCalculator.Calculate is a
//                  read-only query (no state changes anywhere in Shared.
//                  Scheduling), so it requires ModulePermissionVerb.Read,
//                  the lowest tier -- not an invented permission, the
//                  correct existing verb for a read-only operation.
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
using Shared.Scheduling.Abstractions.Rules;
using Shared.Scheduling.Abstractions.Schedule;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.Scheduling.Infrastructure.Authorization;

/// <summary>Enforces a deterministic authorization check before SchedulingScheduleCalculator.Calculate (via SchedulingTelemetry).</summary>
public static class SchedulingAuthorization
{
    private const string Module = "Scheduling";

    /// <summary>
    /// Authorizes <paramref name="caller"/> for "Scheduling.Read", then calculates a schedule via
    /// SchedulingTelemetry.CalculateWithTelemetryAsync. Throws UnauthorizedAccessException if the
    /// caller's role does not grant "Scheduling.Read".
    /// </summary>
    public static async Task<SchedulingSchedule> CalculateWithAuthorizationAsync(
        SchedulingEvaluationContext context,
        DateTime asOfUtc,
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

        return await Telemetry.SchedulingTelemetry.CalculateWithTelemetryAsync(context, asOfUtc, clock, logEventWriter, caller.CorrelationId);
    }
}
