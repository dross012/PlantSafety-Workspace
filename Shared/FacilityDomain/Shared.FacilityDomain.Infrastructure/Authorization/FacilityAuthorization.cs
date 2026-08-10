// ============================================================================
//  Project:        Shared.FacilityDomain.Infrastructure
//  Module:         Authorization
//  File:           FacilityAuthorization.cs
//  Created:        2026-08-10
//  Description:    Phase 9 (Authorization Modernization) deliverable. Wraps
//                  FacilityTelemetry's three inference-rule wrappers (Phase
//                  8, unchanged) with a deterministic authorization check
//                  before invoking each. All three are read-only inference
//                  operations (verified pure in Phases 4/8), so all three
//                  require ModulePermissionVerb.Read -- the correct existing
//                  verb, not an invented one.
//
//                  On deny, throws UnauthorizedAccessException (see
//                  WorkflowEngineAuthorization.cs's header comment for why
//                  this reuses Shared.ErrorHandling's existing
//                  ExceptionClassifier -> ErrorCategory.Unauthorized -> HTTP
//                  401 mapping instead of inventing new error-handling code).
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Checks;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Infrastructure.Governance;
using Shared.Authorization.Infrastructure.Telemetry;
using Shared.Deterministic.Abstractions.Clock;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Regulatory;
using Shared.FacilityDomain.Abstractions.Severity;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.FacilityDomain.Infrastructure.Authorization;

/// <summary>Enforces a deterministic authorization check before each Facility inference-rule wrapper.</summary>
public static class FacilityAuthorization
{
    private const string Module = "Facility";

    /// <summary>Authorizes <paramref name="caller"/> for "Facility.Read", then infers a HazardType via FacilityTelemetry.</summary>
    public static async Task<HazardType> InferHazardTypeWithAuthorizationAsync(
        string eventType,
        string? description,
        CallerContext caller,
        IPermissionResolver permissionResolver,
        IClock clock,
        ILogEventWriter logEventWriter)
    {
        AuthorizeOrThrow(caller, permissionResolver, clock, logEventWriter);
        return await Telemetry.FacilityTelemetry.InferHazardTypeWithTelemetryAsync(eventType, description, clock, logEventWriter, caller.CorrelationId);
    }

    /// <summary>Authorizes <paramref name="caller"/> for "Facility.Read", then infers a SeverityLevel via FacilityTelemetry.</summary>
    public static async Task<SeverityLevel> InferSeverityWithAuthorizationAsync(
        HazardType hazardType,
        int employeeInjuryCount,
        int machineFailureCount,
        int eventChainLength,
        CallerContext caller,
        IPermissionResolver permissionResolver,
        IClock clock,
        ILogEventWriter logEventWriter)
    {
        AuthorizeOrThrow(caller, permissionResolver, clock, logEventWriter);
        return await Telemetry.FacilityTelemetry.InferSeverityWithTelemetryAsync(
            hazardType, employeeInjuryCount, machineFailureCount, eventChainLength, clock, logEventWriter, caller.CorrelationId);
    }

    /// <summary>Authorizes <paramref name="caller"/> for "Facility.Read", then infers RegulatoryJurisdictions via FacilityTelemetry.</summary>
    public static async Task<IReadOnlyList<RegulatoryJurisdiction>> InferRegulatoryJurisdictionsWithAuthorizationAsync(
        HazardType hazardType,
        SeverityLevel severityLevel,
        CallerContext caller,
        IPermissionResolver permissionResolver,
        IClock clock,
        ILogEventWriter logEventWriter)
    {
        AuthorizeOrThrow(caller, permissionResolver, clock, logEventWriter);
        return await Telemetry.FacilityTelemetry.InferRegulatoryJurisdictionsWithTelemetryAsync(hazardType, severityLevel, clock, logEventWriter, caller.CorrelationId);
    }

    private static void AuthorizeOrThrow(CallerContext caller, IPermissionResolver permissionResolver, IClock clock, ILogEventWriter logEventWriter)
    {
        AuthorizationGovernanceEvaluator.EvaluateAndReport(caller, Module, clock, logEventWriter);

        AuthorizationDecision decision = AuthorizationCheck.Evaluate(caller, permissionResolver, Module, ModulePermissionVerb.Read);
        AuthorizationTelemetry.RecordDecision(decision, caller, Module, ModulePermissionVerb.Read.ToString(), clock, logEventWriter);

        if (!decision.IsAuthorized)
        {
            throw new UnauthorizedAccessException(decision.Reason);
        }
    }
}
