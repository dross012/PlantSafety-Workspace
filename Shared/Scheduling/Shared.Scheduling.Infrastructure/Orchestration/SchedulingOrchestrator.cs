// ============================================================================
//  Project:        Shared.Scheduling.Infrastructure
//  Module:         Orchestration
//  File:           SchedulingOrchestrator.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable. The one
//                  public entry point tying every Scheduling layer together
//                  in deterministic, fixed order:
//
//                    1. SchedulingTenant.CalculateWithTenantEnforcementAsync
//                       -- tenant isolation, then authorization, then the
//                       telemetry-wrapped SchedulingScheduleCalculator.Calculate
//                       (Phases 9/11/14).
//                    2. SchedulingEvaluationPipeline.Compose -- deterministic
//                       due-time status rollup and unified-schema (Phase 12)
//                       contract composition.
//                    3. SchedulingGovernanceEvaluator -- governance
//                       validation of the composed contract and rule
//                       decision.
//                    4. SchedulingEventEmitter.Emit -- unified event schema
//                       publication.
//                    5. SchedulingTracing.RecordSpan -- deterministic,
//                       clock-derived span recording for the whole
//                       orchestration.
//
//                  This fixed order never varies by caller, tenant, or
//                  payload -- deterministic rule ordering (Phase 7), applied
//                  at the orchestration layer exactly as every prior phase's
//                  *Tenant.cs -> *Authorization.cs -> *Telemetry.cs chain
//                  already enforces at the domain layer. No new business
//                  logic is introduced here beyond composing the layers
//                  above.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Scheduling.Abstractions.Contracts;
using Shared.Scheduling.Abstractions.Rules;
using Shared.Scheduling.Abstractions.Schedule;
using Shared.Scheduling.Infrastructure.Governance;
using Shared.Scheduling.Infrastructure.Telemetry;
using Shared.Scheduling.Infrastructure.Tenant;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.Scheduling.Infrastructure.Orchestration;

/// <summary>Ties tenant enforcement, authorization, telemetry, evaluation-pipeline composition, governance, event emission, and tracing together in deterministic, fixed order.</summary>
public static class SchedulingOrchestrator
{
    /// <summary>
    /// Evaluates <paramref name="request"/> end-to-end: tenant/authorization-enforced schedule
    /// calculation, unified-schema result composition, governance validation, event emission, and
    /// span tracing. Throws UnauthorizedAccessException on tenant-isolation or authorization denial,
    /// or SchedulingValidationException if <paramref name="request"/>.RequiredActions is empty.
    /// </summary>
    public static async Task<SchedulingEvaluationResultContract> EvaluateAsync(
        SchedulingEvaluationRequestContract request,
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

        if (request.RequiredActions.Count == 0)
        {
            throw new SchedulingValidationException(
                SchedulingErrors.NoRequiredActionsSupplied,
                "Cannot evaluate a scheduling request with zero RequiredActions.");
        }

        DateTime startedAtUtc = clock.UtcNow;

        var context = new SchedulingEvaluationContext(
            request.RequiredActions,
            request.OverdueActionCount,
            request.ActiveMachineGroupCount);

        SchedulingSchedule schedule = await SchedulingTenant.CalculateWithTenantEnforcementAsync(
            context,
            request.AsOfUtc,
            caller,
            tenantContext,
            requestedFacilityId,
            permissionResolver,
            tenantFacilityAccessResolver,
            clock,
            logEventWriter);

        SchedulingEvaluationResultContract result = SchedulingEvaluationPipeline.Compose(schedule, request.Metadata, request.AsOfUtc);

        SchedulingGovernanceEvaluator.EvaluateContractMetadataAndReport(result.Metadata, clock.UtcNow, logEventWriter);
        SchedulingGovernanceEvaluator.EvaluateDueTimesAndReport(result.Schedule.DueTimes, result.Metadata.CorrelationId, clock.UtcNow, logEventWriter);
        SchedulingGovernanceEvaluator.EvaluateRuleDecisionAndReport(result.Schedule.RuleDecision, result.Metadata.CorrelationId, clock.UtcNow, logEventWriter);

        SchedulingEventEmitter.Emit(result, logEventWriter);

        DateTime endedAtUtc = clock.UtcNow;
        SchedulingTracing.RecordSpan("SchedulingOrchestrator.EvaluateAsync", startedAtUtc, endedAtUtc, result.Metadata.CorrelationId, logEventWriter);

        logEventWriter.Write(new LogEvent(
            EventId: SchedulingGovernanceEvents.OrchestrationCompleted,
            Category: LogCategory.Scheduling,
            Severity: LogSeverity.Info,
            Message: $"SchedulingOrchestrator.EvaluateAsync completed for tenant \"{result.Metadata.TenantId}\" in {(endedAtUtc - startedAtUtc).TotalMilliseconds:F1}ms.",
            Payload: new Dictionary<string, object?>
            {
                ["TenantId"] = result.Metadata.TenantId,
                ["DurationMs"] = (endedAtUtc - startedAtUtc).TotalMilliseconds,
                ["Status"] = result.Status.ToString(),
            },
            CorrelationId: result.Metadata.CorrelationId,
            Module: "Scheduling",
            OccurredAtUtc: endedAtUtc));

        return result;
    }
}
