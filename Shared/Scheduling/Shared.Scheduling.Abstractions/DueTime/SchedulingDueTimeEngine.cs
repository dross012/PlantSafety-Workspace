// ============================================================================
//  Project:        Shared.Scheduling.Abstractions
//  Module:         DueTime
//  File:           SchedulingDueTimeEngine.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable. Pure,
//                  deterministic due-time calculation over RequiredAction
//                  (Shared.WorkflowEngine.Abstractions.Actions, Phase C.7).
//                  Takes asOfUtc as an explicit parameter -- never reads
//                  IClock or DateTime.UtcNow directly -- so the same
//                  RequiredAction set and the same asOfUtc always produce
//                  the same DueAtUtc, exactly as Shared.Deterministic's
//                  IClock seam (Phase 2.1) requires of every timestamped
//                  computation in this program.
//
//                  Deterministic timestamp propagation: DueAtUtc is derived
//                  solely from asOfUtc + RequiredAction.DueWithinHours --
//                  no wall-clock read, no randomness. Deterministic metadata
//                  extraction: ActionCode/Description are copied verbatim
//                  from RequiredAction, never reinterpreted or inferred.
// ============================================================================
using Shared.WorkflowEngine.Abstractions.Actions;

namespace Shared.Scheduling.Abstractions.DueTime;

/// <summary>A single RequiredAction's deterministically computed due time.</summary>
public sealed record ScheduledDueTime(
    string ActionCode,
    string Description,
    DateTime DueAtUtc,
    bool IsOverdue);

/// <summary>Deterministic due-time calculation over RequiredAction metadata. No side effects, no invented business logic.</summary>
public static class SchedulingDueTimeEngine
{
    /// <summary>
    /// Calculates a ScheduledDueTime for every entry in <paramref name="requiredActions"/>, in the
    /// exact order supplied. DueAtUtc = asOfUtc + RequiredAction.DueWithinHours; IsOverdue is true when
    /// DueAtUtc has already elapsed as of <paramref name="asOfUtc"/> itself (DueWithinHours &lt;= 0).
    /// </summary>
    public static IReadOnlyList<ScheduledDueTime> CalculateDueTimes(IReadOnlyList<RequiredAction> requiredActions, DateTime asOfUtc)
    {
        ArgumentNullException.ThrowIfNull(requiredActions);

        var results = new List<ScheduledDueTime>(requiredActions.Count);
        foreach (RequiredAction action in requiredActions)
        {
            DateTime dueAtUtc = asOfUtc.AddHours(action.DueWithinHours);
            results.Add(new ScheduledDueTime(
                ActionCode: action.ActionCode,
                Description: action.Description,
                DueAtUtc: dueAtUtc,
                IsOverdue: dueAtUtc <= asOfUtc));
        }

        return results;
    }
}
