// ============================================================================
//  Project:        Shared.SafetyData.Infrastructure
//  Module:         Authorization
//  File:           RoutingAuthorization.cs
//  Created:        2026-08-10
//  Description:    Phase 13 (Routing Modernization) deliverable. Wraps
//                  RoutingTelemetry.RouteWithTelemetryAsync (this phase,
//                  unchanged) with a deterministic authorization check
//                  before invoking it -- exact mirror of
//                  WorkflowEngineAuthorization (Phase 9). EventRoutingEngine.
//                  Route is a read-only query (no state changes -- confirmed
//                  by direct inspection: it only projects/concatenates the
//                  inputs), so it requires ModulePermissionVerb.Read, the
//                  correct existing verb, not an invented one.
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
using Shared.SafetyData.Abstractions.Models.Industries.Eventing;
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.SafetyData.Infrastructure.Authorization;

/// <summary>Enforces a deterministic authorization check before EventRoutingEngine.Route (via RoutingTelemetry).</summary>
public static class RoutingAuthorization
{
    private const string Module = "Routing";

    /// <summary>
    /// Authorizes <paramref name="caller"/> for "Routing.Read", then routes via
    /// RoutingTelemetry.RouteWithTelemetryAsync. Throws UnauthorizedAccessException if the
    /// caller's role does not grant "Routing.Read".
    /// </summary>
    public static async Task<EventRoutingResult> RouteWithAuthorizationAsync(
        EventDefinition eventDefinition,
        FacilityProfile facility,
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

        return await Telemetry.RoutingTelemetry.RouteWithTelemetryAsync(eventDefinition, facility, clock, logEventWriter, caller.CorrelationId);
    }
}
