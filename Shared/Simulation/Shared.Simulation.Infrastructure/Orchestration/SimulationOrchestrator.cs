// ============================================================================
//  Project:        Shared.Simulation.Infrastructure
//  Module:         Orchestration
//  File:           SimulationOrchestrator.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. The one public entry point tying
//                  every Simulation layer together in deterministic, fixed
//                  order, mirroring SafetyGraphOrchestrator (Phase 16)
//                  exactly:
//
//                    1. SimulationTenant.EvaluateWithTenantEnforcementAsync
//                       -- tenant isolation, then authorization, then the
//                       fixed Replay -> Evaluate pipeline (Phases 9/11/17).
//                    2. SimulationGovernanceEvaluator -- governance
//                       validation of the composed contract, scenario shape,
//                       and escalation outcome.
//                    3. SimulationEventEmitter.Emit -- unified event schema
//                       publication.
//                    4. SimulationTracing.RecordSpan -- deterministic,
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
using Shared.Simulation.Abstractions.Contracts;
using Shared.Simulation.Abstractions.Simulation;
using Shared.Simulation.Infrastructure.Governance;
using Shared.Simulation.Infrastructure.Telemetry;
using Shared.Simulation.Infrastructure.Tenant;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.Simulation.Infrastructure.Orchestration;

/// <summary>Ties tenant enforcement, authorization, the Replay-then-Evaluate pipeline, governance, event emission, and tracing together in deterministic, fixed order.</summary>
public static class SimulationOrchestrator
{
    /// <summary>
    /// Evaluates <paramref name="request"/> end-to-end: tenant/authorization-enforced scenario
    /// replay and evaluation, governance validation, event emission, and span tracing. Throws
    /// UnauthorizedAccessException on tenant-isolation or authorization denial, or
    /// SimulationValidationException if <paramref name="request"/>.Scenario carries zero steps.
    /// </summary>
    public static async Task<SimulationEvaluationResultContract> EvaluateAsync(
        SimulationEvaluationRequestContract request,
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

        if (request.Scenario.Steps.Count == 0)
        {
            throw new SimulationValidationException(
                SimulationErrors.EmptyScenario,
                "Cannot evaluate a Simulation request whose Scenario carries zero steps.");
        }

        DateTime startedAtUtc = clock.UtcNow;

        SimulationEvaluationResultContract result = await SimulationTenant.EvaluateWithTenantEnforcementAsync(
            request, caller, tenantContext, requestedFacilityId, permissionResolver, tenantFacilityAccessResolver, clock, logEventWriter);

        SimulationGovernanceEvaluator.EvaluateContractMetadataAndReport(result.Metadata, clock.UtcNow, logEventWriter);
        SimulationGovernanceEvaluator.EvaluateScenarioAndReport(request.Scenario, result.Metadata.CorrelationId, clock.UtcNow, logEventWriter);

        SimulationEventEmitter.Emit(result, logEventWriter);

        DateTime endedAtUtc = clock.UtcNow;
        SimulationTracing.RecordSpan("SimulationOrchestrator.EvaluateAsync", startedAtUtc, endedAtUtc, result.Metadata.CorrelationId, logEventWriter);

        logEventWriter.Write(new LogEvent(
            EventId: SimulationGovernanceEvents.OrchestrationCompleted,
            Category: LogCategory.Simulation,
            Severity: LogSeverity.Info,
            Message: $"SimulationOrchestrator.EvaluateAsync completed for tenant \"{result.Metadata.TenantId}\" in {(endedAtUtc - startedAtUtc).TotalMilliseconds:F1}ms.",
            Payload: new Dictionary<string, object?>
            {
                ["TenantId"] = result.Metadata.TenantId,
                ["DurationMs"] = (endedAtUtc - startedAtUtc).TotalMilliseconds,
                ["OverallPriority"] = result.Outcome.OverallPriority.ToString(),
            },
            CorrelationId: result.Metadata.CorrelationId,
            Module: "Simulation",
            OccurredAtUtc: endedAtUtc));

        return result;
    }
}
