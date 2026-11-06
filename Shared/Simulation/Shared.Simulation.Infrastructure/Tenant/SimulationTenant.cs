// ============================================================================
//  Project:        Shared.Simulation.Infrastructure
//  Module:         Tenant
//  File:           SimulationTenant.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. Wraps SimulationAuthorization.
//                  EvaluateWithAuthorizationAsync (this phase, unchanged)
//                  with a deterministic tenant-isolation check before
//                  invoking it -- mirrors SafetyGraphTenant (Phase 16)
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
using Shared.Simulation.Abstractions.Simulation;
using Shared.Simulation.Infrastructure.Authorization;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.Simulation.Infrastructure.Tenant;

/// <summary>Enforces a deterministic tenant-isolation check before SimulationAuthorization.EvaluateWithAuthorizationAsync.</summary>
public static class SimulationTenant
{
    private const string Module = "Simulation";

    /// <summary>
    /// Evaluates <paramref name="tenantContext"/>'s access to <paramref name="requestedFacilityId"/>, composes
    /// and governance-checks a ContractMetadata, then authorizes and evaluates via SimulationAuthorization.
    /// Throws UnauthorizedAccessException on tenant-isolation denial or authorization denial.
    /// </summary>
    public static async Task<SimulationEvaluationResultContract> EvaluateWithTenantEnforcementAsync(
        SimulationEvaluationRequestContract request,
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

        return await SimulationAuthorization.EvaluateWithAuthorizationAsync(request, caller, permissionResolver, clock, logEventWriter);
    }
}
