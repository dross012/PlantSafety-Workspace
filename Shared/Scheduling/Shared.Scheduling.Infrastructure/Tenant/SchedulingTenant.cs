// ============================================================================
//  Project:        Shared.Scheduling.Infrastructure
//  Module:         Tenant
//  File:           SchedulingTenant.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable. Wraps
//                  SchedulingAuthorization.CalculateWithAuthorizationAsync
//                  (this phase, unchanged) with a deterministic
//                  tenant-isolation check before invoking it -- mirrors
//                  WorkflowEngineTenant (Phase 11) and RoutingTenant
//                  (Phase 13) exactly.
//
//                  requestedFacilityId is threaded in as an explicit
//                  parameter rather than read off SchedulingEvaluationContext,
//                  because SchedulingEvaluationContext carries no
//                  facility/tenant field -- confirmed before writing this
//                  file, same reasoning WorkflowEngineTenant.cs's own header
//                  documents.
//
//                  Also composes a ContractMetadata (Phase 12) from the
//                  resolved TenantContext and runs ContractGovernanceEvaluator
//                  -- the "Scheduling must adopt unified contract schema"
//                  requirement (Phase 14 section 1.7), applied the same way
//                  RoutingTenant.cs (Phase 13) already applies it.
//
//                  On deny, throws UnauthorizedAccessException -- same reuse
//                  of Shared.ErrorHandling's ExceptionClassifier -> 401
//                  mapping every prior phase's *Tenant.cs documents.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Contracts;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Authorization.Infrastructure.Contracts;
using Shared.Authorization.Infrastructure.Governance;
using Shared.Authorization.Infrastructure.Telemetry;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Scheduling.Abstractions.Rules;
using Shared.Scheduling.Abstractions.Schedule;
using Shared.Scheduling.Infrastructure.Authorization;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.Scheduling.Infrastructure.Tenant;

/// <summary>Enforces a deterministic tenant-isolation check before SchedulingAuthorization.CalculateWithAuthorizationAsync.</summary>
public static class SchedulingTenant
{
    private const string Module = "Scheduling";

    /// <summary>
    /// Evaluates <paramref name="tenantContext"/>'s access to <paramref name="requestedFacilityId"/>, composes
    /// and governance-checks a ContractMetadata, then authorizes and calculates via SchedulingAuthorization.
    /// Throws UnauthorizedAccessException on tenant-isolation denial or authorization denial.
    /// </summary>
    public static async Task<SchedulingSchedule> CalculateWithTenantEnforcementAsync(
        SchedulingEvaluationContext context,
        DateTime asOfUtc,
        CallerContext caller,
        TenantContext tenantContext,
        Guid? requestedFacilityId,
        IPermissionResolver permissionResolver,
        ITenantFacilityAccessResolver? tenantFacilityAccessResolver,
        IClock clock,
        ILogEventWriter logEventWriter)
    {
        TenantGovernanceEvaluator.EvaluateContextAndReport(tenantContext, clock, logEventWriter);

        ContractMetadata contractMetadata = ContractMetadataFactory.Create(tenantContext, clock);
        ContractGovernanceEvaluator.EvaluateAndReport(contractMetadata, clock, logEventWriter);

        TenantAccessDecision decision = TenantIsolationCheck.Evaluate(tenantContext, requestedFacilityId, tenantFacilityAccessResolver);
        TenantTelemetry.RecordIsolationDecision(decision, tenantContext, Module, clock, logEventWriter);
        TenantGovernanceEvaluator.EvaluateIsolationDecisionAndReport(decision, requestedFacilityId, tenantContext.CorrelationId, clock, logEventWriter);

        if (!decision.IsAuthorized)
        {
            throw new UnauthorizedAccessException(decision.Reason);
        }

        return await SchedulingAuthorization.CalculateWithAuthorizationAsync(context, asOfUtc, caller, permissionResolver, clock, logEventWriter);
    }
}
