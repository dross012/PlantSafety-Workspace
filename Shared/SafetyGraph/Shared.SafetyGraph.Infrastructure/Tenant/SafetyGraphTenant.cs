// ============================================================================
//  Project:        Shared.SafetyGraph.Infrastructure
//  Module:         Tenant
//  File:           SafetyGraphTenant.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. Wraps SafetyGraphAuthorization.
//                  EvaluateWithAuthorizationAsync (this phase, unchanged)
//                  with a deterministic tenant-isolation check before
//                  invoking it -- mirrors SchedulingTenant/PredictiveTenant
//                  (Phases 14/15) exactly, including their Phase 12
//                  ContractMetadata/ContractGovernanceEvaluator composition.
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
using Shared.SafetyGraph.Abstractions.Graph;
using Shared.SafetyGraph.Infrastructure.Authorization;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.SafetyGraph.Infrastructure.Tenant;

/// <summary>Enforces a deterministic tenant-isolation check before SafetyGraphAuthorization.EvaluateWithAuthorizationAsync.</summary>
public static class SafetyGraphTenant
{
    private const string Module = "SafetyGraph";

    /// <summary>
    /// Evaluates <paramref name="tenantContext"/>'s access to <paramref name="requestedFacilityId"/>, composes
    /// and governance-checks a ContractMetadata, then authorizes and evaluates via SafetyGraphAuthorization.
    /// Throws UnauthorizedAccessException on tenant-isolation denial or authorization denial.
    /// </summary>
    public static async Task<SafetyGraphEvaluationResultContract> EvaluateWithTenantEnforcementAsync(
        SafetyGraphEvaluationRequestContract request,
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

        return await SafetyGraphAuthorization.EvaluateWithAuthorizationAsync(request, caller, permissionResolver, clock, logEventWriter);
    }
}
