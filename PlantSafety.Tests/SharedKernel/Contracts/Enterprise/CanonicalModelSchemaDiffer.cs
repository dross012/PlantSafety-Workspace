// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SharedKernel.Contracts.Enterprise
//  Layer:          Tests (Unit)
//  File:           CanonicalModelSchemaDiffer.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Real, working schema-diff utility for the enterprise canonical model's
//                  lightweight JSON-schema-snapshot contract tests. Compares two JSON documents'
//                  top-level fields (by name and JSON value kind) and reports which fields were
//                  added, removed, or changed -- a straightforward reflection-free JSON comparison,
//                  not an external tool dependency. Isolated, deterministic -- no I/O, no network.
// ============================================================================
using System.Text.Json;

namespace PlantSafety.Tests.SharedKernel.Contracts.Enterprise;

/// <summary>Result of diffing two JSON object schemas: field names added, removed, or changed in kind.</summary>
public sealed record CanonicalModelSchemaDiffResult
{
    /// <summary>Field names present in the newer schema but absent from the older schema.</summary>
    public required IReadOnlyList<string> AddedFields { get; init; }

    /// <summary>Field names present in the older schema but absent from the newer schema.</summary>
    public required IReadOnlyList<string> RemovedFields { get; init; }

    /// <summary>Field names present in both schemas whose JSON value kind (string/number/object/etc.) differs.</summary>
    public required IReadOnlyList<string> ChangedFields { get; init; }

    /// <summary>True when the newer schema is a pure additive superset of the older schema (no removals or kind changes).</summary>
    public bool IsAdditiveOnly => RemovedFields.Count == 0 && ChangedFields.Count == 0;
}

/// <summary>Diffs two serialized JSON object shapes and reports added/removed/changed top-level fields.</summary>
public static class CanonicalModelSchemaDiffer
{
    /// <summary>Diffs <paramref name="olderJson"/> against <paramref name="newerJson"/>, both serialized JSON objects.</summary>
    public static CanonicalModelSchemaDiffResult Diff(string olderJson, string newerJson)
    {
        using var olderDoc = JsonDocument.Parse(olderJson);
        using var newerDoc = JsonDocument.Parse(newerJson);

        var olderFields = olderDoc.RootElement.EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value.ValueKind);
        var newerFields = newerDoc.RootElement.EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value.ValueKind);

        var added = newerFields.Keys.Except(olderFields.Keys).OrderBy(x => x, StringComparer.Ordinal).ToList();
        var removed = olderFields.Keys.Except(newerFields.Keys).OrderBy(x => x, StringComparer.Ordinal).ToList();
        var changed = olderFields.Keys.Intersect(newerFields.Keys)
            .Where(name => olderFields[name] != newerFields[name]
                           // A field that only ever held null in the older sample and now holds a
                           // concrete value (or vice-versa) is not a genuine kind change -- Null is
                           // the JSON representation of every optional canonical field before it is
                           // populated.
                           && olderFields[name] != JsonValueKind.Null
                           && newerFields[name] != JsonValueKind.Null)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();

        return new CanonicalModelSchemaDiffResult
        {
            AddedFields = added,
            RemovedFields = removed,
            ChangedFields = changed,
        };
    }
}
