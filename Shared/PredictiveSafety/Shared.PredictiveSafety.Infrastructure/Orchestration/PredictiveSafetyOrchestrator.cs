// ============================================================================
//  Project:        Shared.PredictiveSafety.Infrastructure
//  Module:         Orchestration
//  File:           PredictiveSafetyOrchestrator.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. The one public entry point tying
//                  every Predictive Safety layer together in deterministic,
//                  fixed order, mirroring SchedulingOrchestrator (Phase 14)
//                  exactly:
//
//                    1. PredictiveTenant.EvaluateWithTenantEnforcementAsync
//                       -- tenant isolation, then authorization, then the
//                       fixed Hazard -> Maintenance -> Routing -> Scheduling
//                       -> Audit pipeline (Phases 9/11/15).
//                    2. PredictiveGovernanceEvaluator -- governance
//                       validation of the composed contract and each
//                       domain's escalation outcome.
//                    3. PredictiveSafetyEventEmitter.Emit -- unified event
//                       schema publication.
//                    4. PredictiveTracing.RecordSpan -- deterministic,
//                       clock-derived span recording for the whole
//                       orchestration.
//
//                  This fixed order never varies by caller, tenant, or
//                  payload. No new business logic is introduced here beyond
//                  composing the layers above.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.PredictiveSafety.Infrastructure.Governance;
using Shared.PredictiveSafety.Infrastructure.Telemetry;
using Shared.PredictiveSafety.Infrastructure.Tenant;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.PredictiveSafety.Infrastructure.Orchestration;

/// <summary>Ties tenant enforcement, authorization, the five-domain evaluation pipeline, governance, event emission, and tracing together in deterministic, fixed order.</summary>
public static class PredictiveSafetyOrchestrator
{
    /// <summary>
    /// Evaluates <paramref name="request"/> end-to-end: tenant/authorization-enforced five-domain
    /// prediction, governance validation, event emission, and span tracing. Throws
    /// UnauthorizedAccessException on tenant-isolation or authorization denial, or
    /// PredictiveSafetyValidationException if <paramref name="request"/>.EventType is empty.
    /// </summary>
    public static async Task<PredictiveSafetyEvaluationResultContract> EvaluateAsync(
        PredictiveSafetyEvaluationRequestContract request,
        CallerContext caller,
        TenantContext tenantContext,
        Guid? requestedFacilityId,
        IPermissionResolver permissionResolver,
        ITenantFacilityAccessResolver? tenantFacilityAccessResolver,
        IClock clock,
        ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        if (string.IsNullOrWhiteSpace(request.EventType))
        {
            throw new PredictiveSafetyValidationException(
                PredictiveSafetyErrors.MissingEventType,
                "Cannot evaluate a Predictive Safety request with an empty EventType.");
        }

        DateTime startedAtUtc = clock.UtcNow;

        PredictiveSafetyEvaluationResultContract result = await PredictiveTenant.EvaluateWithTenantEnforcementAsync(
            request, caller, tenantContext, requestedFacilityId, permissionResolver, tenantFacilityAccessResolver, clock, logEventWriter);

        PredictiveGovernanceEvaluator.EvaluateContractMetadataAndReport(result.Metadata, clock.UtcNow, logEventWriter);

        PredictiveSafetyEventEmitter.Emit(result, logEventWriter);

        DateTime endedAtUtc = clock.UtcNow;
        PredictiveTracing.RecordSpan("PredictiveSafetyOrchestrator.EvaluateAsync", startedAtUtc, endedAtUtc, result.Metadata.CorrelationId, logEventWriter);

        logEventWriter.Write(new LogEvent(
            EventId: PredictiveGovernanceEvents.OrchestrationCompleted,
            Category: LogCategory.PredictiveSafety,
            Severity: LogSeverity.Info,
            Message: $"PredictiveSafetyOrchestrator.EvaluateAsync completed for tenant \"{result.Metadata.TenantId}\" in {(endedAtUtc - startedAtUtc).TotalMilliseconds:F1}ms.",
            Payload: new Dictionary<string, object?>
            {
                ["TenantId"] = result.Metadata.TenantId,
                ["DurationMs"] = (endedAtUtc - startedAtUtc).TotalMilliseconds,
                ["OverallPriority"] = result.OverallPriority.ToString(),
            },
            CorrelationId: result.Metadata.CorrelationId,
            Module: "PredictiveSafety",
            OccurredAtUtc: endedAtUtc));

        return result;
    }
}
