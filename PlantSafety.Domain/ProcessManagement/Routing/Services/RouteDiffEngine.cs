// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           RouteDiffEngine.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Pure, deterministic domain service computing a structural diff between two
//                  RouteVersionSnapshots' step content. Steps are matched by their stable StepId
//                  (preserved across in-place revisions by RouteDefinition.Revise); unmatched
//                  StepIds in the previous snapshot are Deletions, unmatched StepIds in the
//                  current snapshot are Additions, matched StepIds with a changed StepNumber are
//                  Reorders, and matched StepIds with changed Name/Instructions/Resources produce
//                  one Modification row per changed field. Genuinely new algorithmic work -- no
//                  diff concept exists anywhere else in the platform for a step-structured
//                  aggregate. Deterministic: same two snapshots always produce the same ordered
//                  result. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;

namespace PlantSafety.Domain.ProcessManagement.Routing.Services;

/// <summary>Computes a deterministic structural diff between two <see cref="RouteStepSnapshot"/> collections.</summary>
public static class RouteDiffEngine
{
    private const string StepsGroup = "Steps";
    private const string Addition = "Addition";
    private const string Deletion = "Deletion";
    private const string Modification = "Modification";
    private const string Reorder = "Reorder";

    /// <summary>
    /// Computes the ordered list of structural differences between <paramref name="previous"/> and
    /// <paramref name="current"/> step snapshots. Returns an empty list when the two collections
    /// are structurally identical.
    /// </summary>
    public static IReadOnlyList<RouteStepDiffEntry> Diff(
        IReadOnlyList<RouteStepSnapshot> previous,
        IReadOnlyList<RouteStepSnapshot> current)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(current);

        var previousById = previous.ToDictionary(s => s.StepId);
        var currentById = current.ToDictionary(s => s.StepId);

        var entries = new List<RouteStepDiffEntry>();

        foreach (var removed in previous.Where(p => !currentById.ContainsKey(p.StepId)).OrderBy(s => s.StepNumber))
        {
            entries.Add(new RouteStepDiffEntry(
                StepsGroup,
                $"Step {removed.StepNumber}: {removed.Name}",
                Deletion,
                DescribeStep(removed),
                null));
        }

        foreach (var added in current.Where(c => !previousById.ContainsKey(c.StepId)).OrderBy(s => s.StepNumber))
        {
            entries.Add(new RouteStepDiffEntry(
                StepsGroup,
                $"Step {added.StepNumber}: {added.Name}",
                Addition,
                null,
                DescribeStep(added)));
        }

        foreach (var stepId in previousById.Keys.Where(currentById.ContainsKey).OrderBy(id => previousById[id].StepNumber))
        {
            var prev = previousById[stepId];
            var curr = currentById[stepId];

            if (prev.StepNumber != curr.StepNumber)
            {
                entries.Add(new RouteStepDiffEntry(
                    StepsGroup,
                    $"Step position: {curr.Name}",
                    Reorder,
                    $"Position {prev.StepNumber}",
                    $"Position {curr.StepNumber}"));
            }

            if (!string.Equals(prev.Name, curr.Name, StringComparison.Ordinal))
            {
                entries.Add(new RouteStepDiffEntry(StepsGroup, $"Step {curr.StepNumber}: Name", Modification, prev.Name, curr.Name));
            }

            if (!string.Equals(prev.Instructions, curr.Instructions, StringComparison.Ordinal))
            {
                entries.Add(new RouteStepDiffEntry(StepsGroup, $"Step {curr.StepNumber}: Instructions", Modification, prev.Instructions, curr.Instructions));
            }

            if (!prev.Resources.SequenceEqual(curr.Resources, StringComparer.Ordinal))
            {
                entries.Add(new RouteStepDiffEntry(
                    StepsGroup,
                    $"Step {curr.StepNumber}: Resources",
                    Modification,
                    string.Join(", ", prev.Resources),
                    string.Join(", ", curr.Resources)));
            }

            if (prev.EstimatedDurationMinutes != curr.EstimatedDurationMinutes)
            {
                entries.Add(new RouteStepDiffEntry(
                    StepsGroup,
                    $"Step {curr.StepNumber}: Estimated Duration",
                    Modification,
                    $"{prev.EstimatedDurationMinutes} min",
                    $"{curr.EstimatedDurationMinutes} min"));
            }
        }

        return entries;
    }

    private static string DescribeStep(RouteStepSnapshot step)
        => $"{step.Name} ({step.Instructions})";
}
