// ============================================================================
//  Project:        Shared.Scheduling.Abstractions
//  Module:         DueTime
//  File:           SchedulingDueTimeEvaluator.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable. Rolls a
//                  set of ScheduledDueTime results (SchedulingDueTimeEngine)
//                  up into a single deterministic SchedulingDueTimeStatus,
//                  using worst-case-wins: any single overdue entry makes the
//                  whole rollup Overdue, regardless of how many other
//                  entries are OnTrack. Pure function -- no I/O, no
//                  invented thresholds beyond the one documented constant
//                  below.
// ============================================================================
namespace Shared.Scheduling.Abstractions.DueTime;

/// <summary>The deterministic, worst-case-wins rollup status of a set of due times.</summary>
public enum SchedulingDueTimeStatus
{
    /// <summary>No due time is within the due-soon window.</summary>
    OnTrack,

    /// <summary>At least one due time falls within DueSoonThresholdHours, but none are overdue.</summary>
    DueSoon,

    /// <summary>At least one due time has already elapsed.</summary>
    Overdue,
}

/// <summary>Deterministic, worst-case-wins rollup of ScheduledDueTime results into a single SchedulingDueTimeStatus.</summary>
public static class SchedulingDueTimeEvaluator
{
    /// <summary>The number of hours before a due time counts as "due soon" rather than "on track".</summary>
    public const int DueSoonThresholdHours = 4;

    /// <summary>
    /// Evaluates the worst-case status across <paramref name="dueTimes"/> as of <paramref name="asOfUtc"/>.
    /// Overdue beats DueSoon beats OnTrack -- a single overdue or due-soon entry escalates the entire
    /// rollup; no entry can relax a worse status already found.
    /// </summary>
    public static SchedulingDueTimeStatus EvaluateStatus(IReadOnlyList<ScheduledDueTime> dueTimes, DateTime asOfUtc)
    {
        ArgumentNullException.ThrowIfNull(dueTimes);

        bool anyOverdue = false;
        bool anyDueSoon = false;

        foreach (ScheduledDueTime dueTime in dueTimes)
        {
            if (dueTime.IsOverdue)
            {
                anyOverdue = true;
                continue;
            }

            if ((dueTime.DueAtUtc - asOfUtc).TotalHours <= DueSoonThresholdHours)
            {
                anyDueSoon = true;
            }
        }

        if (anyOverdue)
        {
            return SchedulingDueTimeStatus.Overdue;
        }

        return anyDueSoon ? SchedulingDueTimeStatus.DueSoon : SchedulingDueTimeStatus.OnTrack;
    }
}
