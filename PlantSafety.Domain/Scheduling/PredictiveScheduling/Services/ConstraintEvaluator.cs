// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           ConstraintEvaluator.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Phase 5 Predictive Scheduling Engine)
//  SchemaVersion:  2026.1
//  Description:    Domain service orchestrating the predictive scheduling rule pipeline.
//                  Runs the configured IConstraintRule set against a candidate
//                  ScheduleAssignment and aggregates the resulting violations, determining
//                  overall feasibility. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Services;

/// <summary>Orchestrates the constraint rule pipeline and aggregates violations for a candidate assignment.</summary>
public sealed class ConstraintEvaluator : IDomainService
{
    private readonly IReadOnlyList<IConstraintRule> _rules;

    /// <summary>Creates the evaluator with the platform-default rule pipeline (shift/crew, machine, safety).</summary>
    public ConstraintEvaluator() : this(DefaultRules()) { }

    /// <summary>Creates the evaluator with an explicit, ordered rule pipeline.</summary>
    public ConstraintEvaluator(IEnumerable<IConstraintRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        _rules = rules.ToList();
        if (_rules.Count == 0)
            throw new ArgumentException("At least one IConstraintRule must be supplied.", nameof(rules));
    }

    /// <summary>The platform-default rule pipeline: shift/crew, machine availability, then safety.</summary>
    public static IReadOnlyList<IConstraintRule> DefaultRules() =>
        [new ShiftCrewRuleSet(), new MachineAvailabilityRuleSet(), new SafetyRuleSet()];

    /// <summary>
    /// Evaluates <paramref name="assignment"/> for <paramref name="task"/> against every configured
    /// rule and returns the union of all violations raised (empty when the assignment is clean).
    /// </summary>
    public IReadOnlyList<ConstraintViolation> Evaluate(
        SchedulableTask task,
        ScheduleAssignment assignment,
        IReadOnlyList<ResourceAvailabilityWindow> availability,
        IReadOnlyList<ScheduleAssignment> committedAssignments)
    {
        var violations = new List<ConstraintViolation>();
        foreach (var rule in _rules)
            violations.AddRange(rule.Evaluate(task, assignment, availability, committedAssignments));

        return violations;
    }

    /// <summary>True when none of the violations raised for the candidate assignment are Blocking-severity.</summary>
    public bool IsFeasible(
        SchedulableTask task,
        ScheduleAssignment assignment,
        IReadOnlyList<ResourceAvailabilityWindow> availability,
        IReadOnlyList<ScheduleAssignment> committedAssignments)
        => Evaluate(task, assignment, availability, committedAssignments)
            .All(v => v.Severity != ConstraintSeverity.Blocking);
}
