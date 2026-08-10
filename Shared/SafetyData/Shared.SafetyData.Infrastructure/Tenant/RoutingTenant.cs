// ============================================================================
//  Project:        Shared.SafetyData.Infrastructure
//  Module:         Tenant
//  File:           RoutingTenant.cs
//  Created:        2026-08-10
//  Description:    Phase 13 (Routing Modernization) deliverable. Wraps
//                  RoutingAuthorization.RouteWithAuthorizationAsync (this
//                  phase, unchanged) with a deterministic tenant-isolation
//                  check before invoking it -- mirrors WorkflowEngineTenant/
//                  FacilityTenant (Phase 11) exactly.
//
//                  Unlike WorkflowEngineTenant/FacilityTenant/AuditTenant
//                  (all of which had to pass requestedFacilityId: null
//                  because their request contracts carry no facility field),
//                  EventRoutingResult -- and its own input, FacilityProfile --
//                  already carries a real FacilityId. This is the first
//                  module-level tenant wrapper in this program where
//                  TenantIsolationCheck's facility-boundary branch is
//                  actually exercised with real data, not a dormant null.
//
//                  Also composes a ContractMetadata (Phase 12) from the
//                  resolved TenantContext and runs ContractGovernanceEvaluator
//                  -- the "Diagnostics must adopt unified contract schema"
//                  requirement, applied here to the real module this phase
//                  actually modernizes.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Contracts;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Authorization.Infrastructure.Contracts;
using Shared.Authorization.Infrastructure.Governance;
using Shared.Authorization.Infrastructure.Telemetry;
using Shared.Deterministic.Abstractions.Clock;
using Shared.SafetyData.Abstractions.Models.Industries.Eventing;
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;
using Shared.SafetyData.Infrastructure.Authorization;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.SafetyData.Infrastructure.Tenant;

/// <summary>Enforces a deterministic tenant-isolation check (against the routed facility) before RoutingAuthorization.RouteWithAuthorizationAsync.</summary>
public static class RoutingTenant
{
    private const string Module = "Routing";

    /// <summary>
    /// Evaluates <paramref name="tenantContext"/>'s access to <paramref name="facility"/>.FacilityId,
    /// then authorizes and routes via RoutingAuthorization. Throws UnauthorizedAccessException on
    /// tenant-isolation denial or authorization denial.
    /// </summary>
    public static async Task<EventRoutingResult> RouteWithTenantEnforcementAsync(
        EventDefinition eventDefinition,
        FacilityProfile facility,
        CallerContext caller,
        TenantContext tenantContext,
        IPermissionResolver permissionResolver,
        ITenantFacilityAccessResolver? tenantFacilityAccessResolver,
        IClock clock,
        ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(facility);

        TenantGovernanceEvaluator.EvaluateContextAndReport(tenantContext, clock, logEventWriter);

        ContractMetadata contractMetadata = ContractMetadataFactory.Create(tenantContext, clock);
        ContractGovernanceEvaluator.EvaluateAndReport(contractMetadata, clock, logEventWriter);

        TenantAccessDecision decision = TenantIsolationCheck.Evaluate(tenantContext, facility.FacilityId, tenantFacilityAccessResolver);
        TenantTelemetry.RecordIsolationDecision(decision, tenantContext, Module, clock, logEventWriter);
        TenantGovernanceEvaluator.EvaluateIsolationDecisionAndReport(decision, facility.FacilityId, tenantContext.CorrelationId, clock, logEventWriter);

        if (!decision.IsAuthorized)
        {
            throw new UnauthorizedAccessException(decision.Reason);
        }

        return await RoutingAuthorization.RouteWithAuthorizationAsync(eventDefinition, facility, caller, permissionResolver, clock, logEventWriter);
    }
}
