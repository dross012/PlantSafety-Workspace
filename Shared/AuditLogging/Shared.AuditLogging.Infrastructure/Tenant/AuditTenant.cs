// ============================================================================
//  Project:        Shared.AuditLogging.Infrastructure
//  Module:         Tenant
//  File:           AuditTenant.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. TenantScopedAuditLogRepository
//                  decorates AuthorizedAuditLogRepository (Phase 9,
//                  unchanged) with a deterministic tenant-isolation check
//                  before SearchAsync. Like AuthorizedAuditLogRepository
//                  before it, this decorator has no live caller in
//                  PlantSafety.UI.Api today (confirmed in Phase 8/9) -- real,
//                  wired, tested capability, documented honestly as dormant.
//
//                  AuditLogSearchCriteria carries no facility id today
//                  (confirmed before writing this file), so
//                  requestedFacilityId is threaded through the constructor
//                  as an explicit value, same pattern as WorkflowEngineTenant
//                  and FacilityTenant.
// ============================================================================
using Shared.AuditLogging.Abstractions.Models;
using Shared.AuditLogging.Abstractions.Repositories;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Authorization.Infrastructure.Governance;
using Shared.Authorization.Infrastructure.Telemetry;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.AuditLogging.Infrastructure.Tenant;

/// <summary>Decorates an IAuditLogRepository with a deterministic tenant-isolation check before SearchAsync.</summary>
public sealed class TenantScopedAuditLogRepository(
    IAuditLogRepository inner,
    TenantContext tenantContext,
    Guid? requestedFacilityId,
    ITenantFacilityAccessResolver? tenantFacilityAccessResolver,
    IClock clock,
    ILogEventWriter logEventWriter) : IAuditLogRepository
{
    private const string Module = "Audit";

    /// <inheritdoc />
    public async Task<AuditLogSearchResult> SearchAsync(AuditLogSearchCriteria criteria, CancellationToken cancellationToken)
    {
        TenantGovernanceEvaluator.EvaluateContextAndReport(tenantContext, clock, logEventWriter);

        TenantAccessDecision decision = TenantIsolationCheck.Evaluate(tenantContext, requestedFacilityId, tenantFacilityAccessResolver);
        TenantTelemetry.RecordIsolationDecision(decision, tenantContext, Module, clock, logEventWriter);
        TenantGovernanceEvaluator.EvaluateIsolationDecisionAndReport(decision, requestedFacilityId, tenantContext.CorrelationId, clock, logEventWriter);

        if (!decision.IsAuthorized)
        {
            throw new UnauthorizedAccessException(decision.Reason);
        }

        return await inner.SearchAsync(criteria, cancellationToken);
    }
}
