// ============================================================================
//  Project:        Shared.PredictiveSafety.Infrastructure
//  Module:         Tenant
//  File:           PredictiveTenant.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. Wraps PredictiveAuthorization.
//                  EvaluateWithAuthorizationAsync (this phase, unchanged)
//                  with a deterministic tenant-isolation check before
//                  invoking it -- mirrors SchedulingTenant (Phase 14)
//                  exactly, including its Phase 12 ContractMetadata/
//                  ContractGovernanceEvaluator composition.
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
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.PredictiveSafety.Infrastructure.Authorization;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.PredictiveSafety.Infrastructure.Tenant;

/// <summary>Enforces a deterministic tenant-isolation check before PredictiveAuthorization.EvaluateWithAuthorizationAsync.</summary>
public static class PredictiveTenant
{
    private const string Module = "PredictiveSafety";

    /// <summary>
    /// Evaluates <paramref name="tenantContext"/>'s access to <paramref name="requestedFacilityId"/>, composes
    /// and governance-checks a ContractMetadata, then authorizes and evaluates via PredictiveAuthorization.
    /// Throws UnauthorizedAccessException on tenant-isolation denial or authorization denial.
    /// </summary>
    public static async Task<PredictiveSafetyEvaluationResultContract> EvaluateWithTenantEnforcementAsync(
        PredictiveSafetyEvaluationRequestContract request,
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

        return await PredictiveAuthorization.EvaluateWithAuthorizationAsync(request, caller, permissionResolver, clock, logEventWriter);
    }
}
