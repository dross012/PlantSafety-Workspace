// ============================================================================
//  Project:        Shared.FacilityDomain.Infrastructure
//  Module:         Tenant
//  File:           FacilityTenant.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. Wraps FacilityAuthorization's three
//                  inference-rule wrappers (Phase 9, unchanged) with a
//                  deterministic tenant-isolation check before invoking each.
//                  Like FacilityAuthorization and FacilityTelemetry before
//                  it, this wrapper has no live caller in PlantSafety.UI.Api
//                  today (only WorkflowEngineController.Evaluate is a real,
//                  live endpoint -- confirmed across Phases 4/7/8/9) -- it is
//                  real, wired, and tested capability, documented honestly as
//                  dormant rather than pretended live.
//
//                  None of Facility's inference inputs (eventType,
//                  description, HazardType, counts) carry a facility id
//                  today, so requestedFacilityId is threaded through as an
//                  explicit parameter, same as WorkflowEngineTenant.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Authorization.Infrastructure.Governance;
using Shared.Authorization.Infrastructure.Telemetry;
using Shared.Deterministic.Abstractions.Clock;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Regulatory;
using Shared.FacilityDomain.Abstractions.Severity;
using Shared.FacilityDomain.Infrastructure.Authorization;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.FacilityDomain.Infrastructure.Tenant;

/// <summary>Enforces a deterministic tenant-isolation check before each Facility inference-rule wrapper.</summary>
public static class FacilityTenant
{
    private const string Module = "Facility";

    /// <summary>Evaluates tenant isolation, then infers a HazardType via FacilityAuthorization.</summary>
    public static async Task<HazardType> InferHazardTypeWithTenantEnforcementAsync(
        string eventType,
        string? description,
        CallerContext caller,
        TenantContext tenantContext,
        Guid? requestedFacilityId,
        IPermissionResolver permissionResolver,
        ITenantFacilityAccessResolver? tenantFacilityAccessResolver,
        IClock clock,
        ILogEventWriter logEventWriter)
    {
        EnforceOrThrow(tenantContext, requestedFacilityId, tenantFacilityAccessResolver, clock, logEventWriter);
        return await FacilityAuthorization.InferHazardTypeWithAuthorizationAsync(eventType, description, caller, permissionResolver, clock, logEventWriter);
    }

    /// <summary>Evaluates tenant isolation, then infers a SeverityLevel via FacilityAuthorization.</summary>
    public static async Task<SeverityLevel> InferSeverityWithTenantEnforcementAsync(
        HazardType hazardType,
        int employeeInjuryCount,
        int machineFailureCount,
        int eventChainLength,
        CallerContext caller,
        TenantContext tenantContext,
        Guid? requestedFacilityId,
        IPermissionResolver permissionResolver,
        ITenantFacilityAccessResolver? tenantFacilityAccessResolver,
        IClock clock,
        ILogEventWriter logEventWriter)
    {
        EnforceOrThrow(tenantContext, requestedFacilityId, tenantFacilityAccessResolver, clock, logEventWriter);
        return await FacilityAuthorization.InferSeverityWithAuthorizationAsync(
            hazardType, employeeInjuryCount, machineFailureCount, eventChainLength, caller, permissionResolver, clock, logEventWriter);
    }

    /// <summary>Evaluates tenant isolation, then infers RegulatoryJurisdictions via FacilityAuthorization.</summary>
    public static async Task<IReadOnlyList<RegulatoryJurisdiction>> InferRegulatoryJurisdictionsWithTenantEnforcementAsync(
        HazardType hazardType,
        SeverityLevel severityLevel,
        CallerContext caller,
        TenantContext tenantContext,
        Guid? requestedFacilityId,
        IPermissionResolver permissionResolver,
        ITenantFacilityAccessResolver? tenantFacilityAccessResolver,
        IClock clock,
        ILogEventWriter logEventWriter)
    {
        EnforceOrThrow(tenantContext, requestedFacilityId, tenantFacilityAccessResolver, clock, logEventWriter);
        return await FacilityAuthorization.InferRegulatoryJurisdictionsWithAuthorizationAsync(hazardType, severityLevel, caller, permissionResolver, clock, logEventWriter);
    }

    private static void EnforceOrThrow(
        TenantContext tenantContext, Guid? requestedFacilityId, ITenantFacilityAccessResolver? resolver, IClock clock, ILogEventWriter logEventWriter)
    {
        TenantGovernanceEvaluator.EvaluateContextAndReport(tenantContext, clock, logEventWriter);

        TenantAccessDecision decision = TenantIsolationCheck.Evaluate(tenantContext, requestedFacilityId, resolver);
        TenantTelemetry.RecordIsolationDecision(decision, tenantContext, Module, clock, logEventWriter);
        TenantGovernanceEvaluator.EvaluateIsolationDecisionAndReport(decision, requestedFacilityId, tenantContext.CorrelationId, clock, logEventWriter);

        if (!decision.IsAuthorized)
        {
            throw new UnauthorizedAccessException(decision.Reason);
        }
    }
}
