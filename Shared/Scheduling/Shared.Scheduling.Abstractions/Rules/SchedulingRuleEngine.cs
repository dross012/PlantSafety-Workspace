// ============================================================================
//  Project:        Shared.Scheduling.Abstractions
//  Module:         Rules
//  File:           SchedulingRuleEngine.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable. The
//                  first real Scheduling rule engine in this program --
//                  a pure, deterministic function over an already-produced
//                  set of RequiredActions (Shared.WorkflowEngine.Abstractions.
//                  Actions.RequiredAction, Phase C.7 -- the one real "due
//                  time" concept that already exists in this codebase; see
//                  Phase 14 investigation notes). Mirrors WorkflowEngine.
//                  Evaluate (Phase C.7) exactly: no IClock, no I/O, no
//                  randomness -- same inputs always produce the same
//                  outputs.
//
//                  Deterministic ordering: RequiredActions are walked in
//                  the exact order supplied by the caller (never
//                  re-sorted), and AppliedRuleCodes are appended in a fixed
//                  literal order below -- never conditionally reordered.
//
//                  Additive accumulation: TotalRequiredActionCount and
//                  TotalEstimatedEffortHours both sum monotonically across
//                  every RequiredAction; no rule ever subtracts or cancels
//                  a prior rule's contribution (Phase 14's "no cancellation
//                  rules" requirement).
//
//                  Worst-case-wins: WorstCaseDueWithinHours is the minimum
//                  DueWithinHours across all RequiredActions (the soonest
//                  deadline "wins" and drives RequiresImmediateScheduling),
//                  and OverdueActionCount/ActiveMachineGroupCount escalate
//                  the decision but never relax it -- no rule here can make
//                  a more urgent decision less urgent (Phase 14's
//                  "no priority overrides" requirement).
//
//                  No ML model, no probabilistic inference, no weighted
//                  scoring, no averaging -- every branch below is a
//                  deterministic threshold comparison over already-known
//                  inputs.
// ============================================================================
using Shared.WorkflowEngine.Abstractions.Actions;

namespace Shared.Scheduling.Abstractions.Rules;

/// <summary>The deterministic inputs SchedulingRuleEngine.Evaluate reacts to.</summary>
public sealed record SchedulingEvaluationContext(
    IReadOnlyList<RequiredAction> RequiredActions,
    int OverdueActionCount,
    int ActiveMachineGroupCount);

/// <summary>The deterministic output of a scheduling rule evaluation.</summary>
public sealed record SchedulingRuleDecision(
    int TotalRequiredActionCount,
    int TotalEstimatedEffortHours,
    int WorstCaseDueWithinHours,
    bool RequiresImmediateScheduling,
    IReadOnlyList<string> AppliedRuleCodes);

/// <summary>Deterministic, pure rule evaluation over a SchedulingEvaluationContext. No side effects, no randomness, no invented business logic.</summary>
public static class SchedulingRuleEngine
{
    /// <summary>The due-within-hours threshold at or below which an action is treated as requiring immediate scheduling.</summary>
    public const int ImmediateSchedulingThresholdHours = 0;

    /// <summary>
    /// Evaluates <paramref name="context"/> deterministically: additively accumulates required-action
    /// counts and estimated effort, and applies worst-case-wins across DueWithinHours, OverdueActionCount,
    /// and ActiveMachineGroupCount to decide whether immediate scheduling is required. Same input always
    /// produces the same output.
    /// </summary>
    public static SchedulingRuleDecision Evaluate(SchedulingEvaluationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var appliedRuleCodes = new List<string>();

        // Additive accumulation: walk RequiredActions in caller-supplied order, summing monotonically.
        int totalRequiredActionCount = 0;
        int totalEstimatedEffortHours = 0;
        int worstCaseDueWithinHours = int.MaxValue;

        foreach (RequiredAction action in context.RequiredActions)
        {
            totalRequiredActionCount++;
            totalEstimatedEffortHours += Math.Max(action.DueWithinHours, 0);

            if (action.DueWithinHours < worstCaseDueWithinHours)
            {
                worstCaseDueWithinHours = action.DueWithinHours;
            }
        }

        if (totalRequiredActionCount > 0)
        {
            appliedRuleCodes.Add("REQUIRED-ACTION-ACCUMULATION");
        }

        // Worst-case-wins: overdue actions and active machine-group exposure only ever escalate,
        // never relax, the immediate-scheduling decision.
        bool requiresImmediateScheduling = worstCaseDueWithinHours <= ImmediateSchedulingThresholdHours;

        if (context.OverdueActionCount > 0)
        {
            requiresImmediateScheduling = true;
            appliedRuleCodes.Add("OVERDUE-ACTION-ESCALATION");
        }

        if (context.ActiveMachineGroupCount > 0 && worstCaseDueWithinHours <= 24)
        {
            requiresImmediateScheduling = true;
            appliedRuleCodes.Add("ACTIVE-MACHINE-GROUP-ESCALATION");
        }

        if (requiresImmediateScheduling)
        {
            appliedRuleCodes.Add("IMMEDIATE-SCHEDULING-REQUIRED");
        }

        return new SchedulingRuleDecision(
            TotalRequiredActionCount: totalRequiredActionCount,
            TotalEstimatedEffortHours: totalEstimatedEffortHours,
            WorstCaseDueWithinHours: worstCaseDueWithinHours,
            RequiresImmediateScheduling: requiresImmediateScheduling,
            AppliedRuleCodes: appliedRuleCodes);
    }
}
