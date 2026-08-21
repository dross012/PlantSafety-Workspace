// ============================================================================
//  Project:        Shared.AuditLogging.Infrastructure
//  Module:         Authorization
//  File:           AuditAuthorization.cs
//  Created:        2026-08-10
//  Description:    Phase 9 (Authorization Modernization) deliverable.
//                  AuthorizedAuditLogRepository decorates TelemetryAuditLogRepository
//                  (Phase 8, unchanged) with a deterministic authorization
//                  check before SearchAsync. Audit search is read-only (the
//                  entire IAuditLogRepository surface is read-only by design
//                  -- confirmed in Phase 5), so it requires
//                  ModulePermissionVerb.Read -- the correct existing verb,
//                  not an invented one. Adds no new method to
//                  IAuditLogRepository and no new business logic.
//
//                  On deny, throws UnauthorizedAccessException (see
//                  Shared.WorkflowEngine.Infrastructure.Authorization.
//                  WorkflowEngineAuthorization's header comment in the outer
//                  modernization program repository for why this reuses
//                  Shared.ErrorHandling's existing ExceptionClassifier ->
//                  ErrorCategory.Unauthorized -> HTTP 401 mapping instead of
//                  inventing new error-handling code).
// ============================================================================
using Shared.AuditLogging.Abstractions.Models;
using Shared.AuditLogging.Abstractions.Repositories;
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Checks;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Infrastructure.Governance;
using Shared.Authorization.Infrastructure.Telemetry;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.AuditLogging.Infrastructure.Authorization;

/// <summary>Decorates an IAuditLogRepository with a deterministic authorization check before SearchAsync.</summary>
public sealed class AuthorizedAuditLogRepository(
    IAuditLogRepository inner,
    CallerContext caller,
    IPermissionResolver permissionResolver,
    IClock clock,
    ILogEventWriter logEventWriter) : IAuditLogRepository
{
    private const string Module = "Audit";

    /// <inheritdoc />
    public async Task<AuditLogSearchResult> SearchAsync(AuditLogSearchCriteria criteria, CancellationToken cancellationToken)
    {
        AuthorizationGovernanceEvaluator.EvaluateAndReport(caller, Module, clock, logEventWriter);

        AuthorizationDecision decision = AuthorizationCheck.Evaluate(caller, permissionResolver, Module, ModulePermissionVerb.Read);
        AuthorizationTelemetry.RecordDecision(decision, caller, Module, ModulePermissionVerb.Read.ToString(), clock, logEventWriter);

        if (!decision.IsAuthorized)
        {
            throw new UnauthorizedAccessException(decision.Reason);
        }

        return await inner.SearchAsync(criteria, cancellationToken);
    }
}
