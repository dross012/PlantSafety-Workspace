// ============================================================================
//  Project:        Shared.Scheduling.Abstractions
//  Module:         Contracts
//  File:           SchedulingDto.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable. Flat,
//                  serialization-friendly DTOs projected from the domain
//                  types (SchedulingSchedule, ScheduledDueTime) for API/UI
//                  consumption -- deterministic mapping only, no
//                  reinterpretation of the underlying rule/due-time
//                  decisions.
// ============================================================================
using Shared.Scheduling.Abstractions.DueTime;
using Shared.Scheduling.Abstractions.Schedule;

namespace Shared.Scheduling.Abstractions.Contracts;

/// <summary>Flat DTO projection of a ScheduledDueTime.</summary>
public sealed record SchedulingDueTimeDto(
    string ActionCode,
    string Description,
    DateTime DueAtUtc,
    bool IsOverdue);

/// <summary>Flat DTO projection of a SchedulingSchedule.</summary>
public sealed record SchedulingScheduleDto(
    IReadOnlyList<SchedulingDueTimeDto> DueTimes,
    int TotalRequiredActionCount,
    int TotalEstimatedEffortHours,
    int WorstCaseDueWithinHours,
    bool RequiresImmediateScheduling,
    DateTime EarliestDueAtUtc,
    string Status);

/// <summary>Deterministic, pure mapping from domain scheduling types to their DTO projections.</summary>
public static class SchedulingDtoMapper
{
    /// <summary>Projects <paramref name="schedule"/> and <paramref name="status"/> into a SchedulingScheduleDto.</summary>
    public static SchedulingScheduleDto ToDto(SchedulingSchedule schedule, SchedulingDueTimeStatus status)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        var dueTimes = new List<SchedulingDueTimeDto>(schedule.DueTimes.Count);
        foreach (ScheduledDueTime dueTime in schedule.DueTimes)
        {
            dueTimes.Add(new SchedulingDueTimeDto(dueTime.ActionCode, dueTime.Description, dueTime.DueAtUtc, dueTime.IsOverdue));
        }

        return new SchedulingScheduleDto(
            DueTimes: dueTimes,
            TotalRequiredActionCount: schedule.RuleDecision.TotalRequiredActionCount,
            TotalEstimatedEffortHours: schedule.RuleDecision.TotalEstimatedEffortHours,
            WorstCaseDueWithinHours: schedule.RuleDecision.WorstCaseDueWithinHours,
            RequiresImmediateScheduling: schedule.RuleDecision.RequiresImmediateScheduling,
            EarliestDueAtUtc: schedule.EarliestDueAtUtc,
            Status: status.ToString());
    }
}
