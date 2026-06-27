// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Deterministic pipeline for filtering, sorting, and grouping
//   DiagnosticResult collections. Each stage is a pure function — it does not
//   mutate inputs and produces identical output for identical inputs.
//   Canonical ordering per Part XI-A: Severity → Category → Rule → Target.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI-A
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;

using PlantSafety.UI.Modules.DiagnosticsViewer.Models;
using PlantSafety.UI.Modules.DiagnosticsViewer.State;

namespace PlantSafety.UI.Modules.DiagnosticsViewer.Pipelines;

/// <summary>
/// Stateless deterministic pipeline that applies filter, sort, and grouping
/// transformations to a <see cref="DiagnosticResult"/> collection.
/// All stages are pure functions — no side effects, no input mutation.
/// </summary>
public static class DiagnosticFilterPipeline
{
    /// <summary>
    /// Applies the full pipeline: filter → sort → materialize.
    /// </summary>
    /// <param name="source">The immutable source collection from <see cref="DiagnosticsSessionState"/>.</param>
    /// <param name="filter">The active filter criteria.</param>
    /// <param name="sort">The active sort state.</param>
    /// <returns>
    /// A new read-only list with all pipeline stages applied.
    /// Input collection is never mutated.
    /// </returns>
    public static IReadOnlyList<DiagnosticResult> Apply(
        IReadOnlyList<DiagnosticResult> source,
        FilterState filter,
        SortState sort)
    {
        if (source is null || source.Count == 0)
            return [];

        var query = source.AsEnumerable();

        query = ApplyFilter(query, filter);
        query = ApplySort(query, sort);

        return query.ToList().AsReadOnly();
    }

    /// <summary>
    /// Groups a pre-filtered result set by the canonical ordering:
    /// Severity → Category → RuleCode → Target.
    /// </summary>
    /// <param name="results">The filtered and sorted result collection.</param>
    /// <returns>
    /// A dictionary keyed by severity containing grouped results.
    /// Ordering within each severity group is: Category → RuleCode → Target.
    /// </returns>
    public static IReadOnlyDictionary<DiagnosticSeverity, IReadOnlyList<DiagnosticResult>> GroupBySeverity(
        IReadOnlyList<DiagnosticResult> results)
    {
        return results
            .GroupBy(r => r.Severity)
            .OrderBy(g => (int)g.Key)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<DiagnosticResult>)g
                    .OrderBy(r => r.Category, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.RuleCode, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(r => r.Target, StringComparer.OrdinalIgnoreCase)
                    .ToList()
                    .AsReadOnly());
    }

    /// <summary>
    /// Computes comparison classifications between a current and baseline result set.
    /// Uses RuleCode + Target + Severity as the matching key.
    /// </summary>
    /// <param name="current">The current diagnostic results.</param>
    /// <param name="baseline">The baseline diagnostic results.</param>
    /// <returns>
    /// A dictionary mapping each result to its comparison classification.
    /// </returns>
    public static IReadOnlyDictionary<DiagnosticResult, ComparisonClassification> ComputeComparison(
        IReadOnlyList<DiagnosticResult> current,
        IReadOnlyList<DiagnosticResult> baseline)
    {
        var baselineIndex = baseline.ToLookup(
            r => (r.RuleCode, r.Target),
            StringComparer.OrdinalIgnoreCase.Equals(string.Empty, string.Empty)
                ? null
                : null);

        var result = new Dictionary<DiagnosticResult, ComparisonClassification>();

        foreach (var item in current)
        {
            var baselineMatches = baseline
                .Where(b => string.Equals(b.RuleCode, item.RuleCode, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(b.Target, item.Target, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (baselineMatches.Count == 0)
            {
                result[item] = ComparisonClassification.New;
            }
            else if (baselineMatches.Any(b => b.Severity == item.Severity))
            {
                result[item] = ComparisonClassification.Unchanged;
            }
            else
            {
                result[item] = ComparisonClassification.Changed;
            }
        }

        return result;
    }

    private static IEnumerable<DiagnosticResult> ApplyFilter(
        IEnumerable<DiagnosticResult> query,
        FilterState filter)
    {
        if (filter.Severities.Count > 0)
            query = query.Where(r => filter.Severities.Contains(r.Severity));

        if (!string.IsNullOrWhiteSpace(filter.CategoryFilter))
            query = query.Where(r => r.Category.Contains(filter.CategoryFilter, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(filter.RuleCodeFilter))
            query = query.Where(r => r.RuleCode.StartsWith(filter.RuleCodeFilter, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(filter.TargetFilter))
            query = query.Where(r => r.Target.Contains(filter.TargetFilter, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm;
            query = query.Where(r =>
                r.Message.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                r.RuleCode.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                r.SuggestedFix.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        return query;
    }

    private static IEnumerable<DiagnosticResult> ApplySort(
        IEnumerable<DiagnosticResult> query,
        SortState sort)
    {
        return sort.SortField switch
        {
            DiagnosticSortField.Severity => sort.IsDescending
                ? query.OrderByDescending(r => (int)r.Severity)
                : query.OrderBy(r => (int)r.Severity),

            DiagnosticSortField.Category => sort.IsDescending
                ? query.OrderByDescending(r => r.Category, StringComparer.OrdinalIgnoreCase)
                : query.OrderBy(r => r.Category, StringComparer.OrdinalIgnoreCase),

            DiagnosticSortField.RuleCode => sort.IsDescending
                ? query.OrderByDescending(r => r.RuleCode, StringComparer.OrdinalIgnoreCase)
                : query.OrderBy(r => r.RuleCode, StringComparer.OrdinalIgnoreCase),

            DiagnosticSortField.Target => sort.IsDescending
                ? query.OrderByDescending(r => r.Target, StringComparer.OrdinalIgnoreCase)
                : query.OrderBy(r => r.Target, StringComparer.OrdinalIgnoreCase),

            DiagnosticSortField.Timestamp => sort.IsDescending
                ? query.OrderByDescending(r => r.TimestampUtc)
                : query.OrderBy(r => r.TimestampUtc),

            _ => query.OrderBy(r => (int)r.Severity)
        };
    }
}
