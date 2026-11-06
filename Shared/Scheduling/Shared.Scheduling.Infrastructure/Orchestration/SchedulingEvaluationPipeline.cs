// ============================================================================
//  Project:        Shared.Scheduling.Infrastructure
//  Module:         Orchestration
//  File:           SchedulingEvaluationPipeline.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable. The
//                  deterministic composition step between a raw
//                  SchedulingSchedule (SchedulingScheduleCalculator, Phase
//                  14 Abstractions) and the unified-schema (Phase 12)
//                  SchedulingEvaluationResultContract SchedulingOrchestrator
//                  returns. Deterministic rule ordering only (Phase 7):
//                  status rollup, then contract composition, in that fixed
//                  order, every time -- no branching on caller identity, no
//                  invented business logic beyond what SchedulingSchedule
//                  already carries.
// ============================================================================
using Shared.Scheduling.Abstractions.Contracts;
using Shared.Scheduling.Abstractions.DueTime;
using Shared.Scheduling.Abstractions.Schedule;

namespace Shared.Scheduling.Infrastructure.Orchestration;

/// <summary>Deterministically composes a SchedulingSchedule into a unified-schema SchedulingEvaluationResultContract.</summary>
public static class SchedulingEvaluationPipeline
{
    /// <summary>
    /// Rolls up <paramref name="schedule"/>'s due-time status as of <paramref name="asOfUtc"/>, then wraps
    /// it and the schedule itself in a SchedulingEvaluationResultContract carrying <paramref name="metadata"/>.
    /// </summary>
    public static SchedulingEvaluationResultContract Compose(
        SchedulingSchedule schedule,
        SchedulingContractMetadata metadata,
        DateTime asOfUtc)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        ArgumentNullException.ThrowIfNull(metadata);

        SchedulingDueTimeStatus status = SchedulingDueTimeEvaluator.EvaluateStatus(schedule.DueTimes, asOfUtc);

        return new SchedulingEvaluationResultContract(metadata, schedule, status);
    }
}
