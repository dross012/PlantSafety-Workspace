// ============================================================================
//  Project:        Shared.SafetyGraph.Infrastructure
//  Module:         Orchestration
//  File:           SafetyGraphOrchestrator.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. The one public entry point tying
//                  every Safety Graph layer together in deterministic, fixed
//                  order, mirroring SchedulingOrchestrator/PredictiveSafetyOrchestrator
//                  (Phases 14/15) exactly:
//
//                    1. SafetyGraphTenant.EvaluateWithTenantEnforcementAsync
//                       -- tenant isolation, then authorization, then the
//                       fixed Construct -> Infer pipeline (Phases 9/11/16).
//                    2. SafetyGraphGovernanceEvaluator -- governance
//                       validation of the composed contract, graph
//                       structure, and escalation outcome.
//                    3. SafetyGraphEventEmitter.Emit -- unified event schema
//                       publication.
//                    4. SafetyGraphTracing.RecordSpan -- deterministic,
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
using Shared.SafetyGraph.Abstractions.Contracts;
using Shared.SafetyGraph.Abstractions.Graph;
using Shared.SafetyGraph.Infrastructure.Governance;
using Shared.SafetyGraph.Infrastructure.Telemetry;
using Shared.SafetyGraph.Infrastructure.Tenant;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.SafetyGraph.Infrastructure.Orchestration;

/// <summary>Ties tenant enforcement, authorization, the Construct-then-Infer pipeline, governance, event emission, and tracing together in deterministic, fixed order.</summary>
public static class SafetyGraphOrchestrator
{
    /// <summary>
    /// Evaluates <paramref name="request"/> end-to-end: tenant/authorization-enforced graph
    /// construction and inference, governance validation, event emission, and span tracing. Throws
    /// UnauthorizedAccessException on tenant-isolation or authorization denial, or
    /// SafetyGraphValidationException if <paramref name="request"/>.ConstructionRequest.FacilityName is empty.
    /// </summary>
    public static async Task<SafetyGraphEvaluationResultContract> EvaluateAsync(
        SafetyGraphEvaluationRequestContract request,
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

        if (string.IsNullOrWhiteSpace(request.ConstructionRequest.FacilityName))
        {
            throw new SafetyGraphValidationException(
                SafetyGraphErrors.MissingFacilityName,
                "Cannot evaluate a Safety Graph request with an empty FacilityName.");
        }

        DateTime startedAtUtc = clock.UtcNow;

        SafetyGraphEvaluationResultContract result = await SafetyGraphTenant.EvaluateWithTenantEnforcementAsync(
            request, caller, tenantContext, requestedFacilityId, permissionResolver, tenantFacilityAccessResolver, clock, logEventWriter);

        SafetyGraphGovernanceEvaluator.EvaluateContractMetadataAndReport(result.Metadata, clock.UtcNow, logEventWriter);
        SafetyGraphGovernanceEvaluator.EvaluateGraphStructureAndReport(result.Graph, result.Metadata.CorrelationId, clock.UtcNow, logEventWriter);

        SafetyGraphEventEmitter.Emit(result, logEventWriter);

        DateTime endedAtUtc = clock.UtcNow;
        SafetyGraphTracing.RecordSpan("SafetyGraphOrchestrator.EvaluateAsync", startedAtUtc, endedAtUtc, result.Metadata.CorrelationId, logEventWriter);

        logEventWriter.Write(new LogEvent(
            EventId: SafetyGraphGovernanceEvents.OrchestrationCompleted,
            Category: LogCategory.SafetyGraph,
            Severity: LogSeverity.Info,
            Message: $"SafetyGraphOrchestrator.EvaluateAsync completed for tenant \"{result.Metadata.TenantId}\" in {(endedAtUtc - startedAtUtc).TotalMilliseconds:F1}ms.",
            Payload: new Dictionary<string, object?>
            {
                ["TenantId"] = result.Metadata.TenantId,
                ["DurationMs"] = (endedAtUtc - startedAtUtc).TotalMilliseconds,
                ["OverallPriority"] = result.Inference.OverallPriority.ToString(),
            },
            CorrelationId: result.Metadata.CorrelationId,
            Module: "SafetyGraph",
            OccurredAtUtc: endedAtUtc));

        return result;
    }
}
