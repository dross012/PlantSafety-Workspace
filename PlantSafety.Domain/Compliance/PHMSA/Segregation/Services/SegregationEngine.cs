// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Segregation
//  Layer:          Domain
//  File:           SegregationEngine.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Pure, stateless, deterministic domain service that evaluates a proposed
//                  multi-hazmat load for segregation incompatibilities and produces a recommended
//                  load plan. There is no persisted aggregate for Segregation -- it is a pure
//                  calculation over caller-supplied load items.
//
//                  ILLUSTRATIVE SIMPLIFICATION NOTICE: the compatibility table below is a
//                  deliberately small, clearly-labeled illustration of the segregation logic found
//                  in 49 CFR 177.848 ("Segregation table for hazardous materials") and its
//                  associated notes. This is NOT a verbatim reproduction of the regulatory table
//                  and MUST NOT be treated as a compliance-certified implementation without
//                  legal/regulatory review. The structure (a keyed pair -> severity/reason lookup)
//                  is intended so a real, complete 49 CFR 177.848 table can be substituted in
//                  without changing the public API.
//
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Segregation.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Segregation.ValueObjects;

namespace PlantSafety.Domain.Compliance.PHMSA.Segregation.Services;

/// <summary>Input to <see cref="SegregationEngine.EvaluateSegregation"/>.</summary>
public sealed record SegregationEvaluationInput(
    IReadOnlyList<(HazmatMaterialId MaterialId, HazardClass HazardClass)> LoadItems);

/// <summary>Result of <see cref="SegregationEngine.EvaluateSegregation"/>.</summary>
public sealed record SegregationEvaluationResult(
    bool IsCompliant,
    IReadOnlyList<SegregationViolation> Violations,
    IReadOnlyList<string> RecommendedLoadPlan);

/// <summary>
/// Pure, stateless, deterministic domain service that evaluates hazard-class segregation
/// incompatibilities across a proposed load. See file header for the scope of the illustrative
/// simplification applied here.
/// </summary>
public sealed class SegregationEngine : IDomainService
{
    /// <summary>
    /// ILLUSTRATIVE SIMPLIFICATION of 49 CFR 177.848: a small, unordered-pair-keyed segregation
    /// compatibility table. Real regulatory coverage spans all hazard class/division combinations;
    /// this table intentionally covers only a representative subset so the structure can be
    /// extended (or replaced by a fully populated table) without changing callers.
    /// </summary>
    private static readonly Dictionary<(string, string), (string Reason, SegregationSeverity Severity)> SegregationTable =
        BuildSegregationTable();

    /// <summary>Evaluates a proposed load for segregation incompatibilities across all item pairs.</summary>
    public SegregationEvaluationResult EvaluateSegregation(SegregationEvaluationInput input)
    {
        if (input is null || input.LoadItems is null || input.LoadItems.Count == 0)
            return new SegregationEvaluationResult(true, [], []);

        var violations = new List<SegregationViolation>();
        var seenPairs = new HashSet<(string, string)>();

        for (var i = 0; i < input.LoadItems.Count; i++)
        {
            for (var j = i + 1; j < input.LoadItems.Count; j++)
            {
                var classA = input.LoadItems[i].HazardClass;
                var classB = input.LoadItems[j].HazardClass;

                var keyA = classA.ToString() ?? string.Empty;
                var keyB = classB.ToString() ?? string.Empty;
                if (string.Equals(keyA, keyB, StringComparison.OrdinalIgnoreCase))
                    continue;

                var lookupKey = NormalizedKey(keyA, keyB);
                if (!seenPairs.Add(lookupKey))
                    continue; // Already recorded this class-pair violation (dedup across load items).

                if (SegregationTable.TryGetValue(lookupKey, out var entry))
                {
                    violations.Add(new SegregationViolation(classA, classB, entry.Reason, entry.Severity));
                }
            }
        }

        var recommendedLoadPlan = BuildRecommendedLoadPlan(violations);

        return new SegregationEvaluationResult(
            IsCompliant: violations.Count == 0,
            Violations: violations,
            RecommendedLoadPlan: recommendedLoadPlan);
    }

    private static IReadOnlyList<string> BuildRecommendedLoadPlan(IReadOnlyList<SegregationViolation> violations)
    {
        if (violations.Count == 0)
            return [];

        var recommendations = new List<string>();
        foreach (var violation in violations)
        {
            var classAKey = violation.ClassA.ToString();
            var classBKey = violation.ClassB.ToString();

            recommendations.Add(violation.Severity == SegregationSeverity.Prohibited
                ? $"Load Class {classAKey} materials in a separate compartment from Class {classBKey} materials -- these must not be transported together ({violation.Reason})."
                : $"Where practical, position Class {classAKey} materials away from Class {classBKey} materials ({violation.Reason}).");
        }

        return recommendations;
    }

    private static (string, string) NormalizedKey(string a, string b)
        => string.CompareOrdinal(a, b) <= 0 ? (a, b) : (b, a);

    private static Dictionary<(string, string), (string Reason, SegregationSeverity Severity)> BuildSegregationTable()
    {
        var table = new Dictionary<(string, string), (string, SegregationSeverity)>();

        void Add(string classA, string classB, string reason, SegregationSeverity severity)
            => table[NormalizedKey(classA, classB)] = (reason, severity);

        // Class 1 (Explosives, illustrative divisions) segregated from Class 3 (Flammable liquid).
        Add("1.1", "3", "Explosives must be segregated from flammable liquids.", SegregationSeverity.Prohibited);
        Add("1.2", "3", "Explosives must be segregated from flammable liquids.", SegregationSeverity.Prohibited);
        Add("1.3", "3", "Explosives must be segregated from flammable liquids.", SegregationSeverity.Prohibited);

        // Class 1 (Explosives) segregated from Class 8 (Corrosive).
        Add("1.1", "8", "Explosives must be segregated from corrosive materials.", SegregationSeverity.Prohibited);
        Add("1.2", "8", "Explosives must be segregated from corrosive materials.", SegregationSeverity.Prohibited);
        Add("1.3", "8", "Explosives must be segregated from corrosive materials.", SegregationSeverity.Prohibited);

        // Class 5.1 (Oxidizer) vs Class 3 (Flammable liquid).
        Add("5.1", "3", "Oxidizers must be kept away from flammable liquids to avoid combustion risk.", SegregationSeverity.Prohibited);

        // Class 8 (Corrosive) vs Class 4.3 (Water-reactive / dangerous when wet).
        Add("8", "4.3", "Corrosive materials must be kept away from water-reactive materials.", SegregationSeverity.Advisory);

        return table;
    }
}
