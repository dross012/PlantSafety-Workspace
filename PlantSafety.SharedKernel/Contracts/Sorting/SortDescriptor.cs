// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Contracts
//  Layer:          SharedKernel
//  File:           SortDescriptor.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Canonical sort descriptor for dynamic ORDER BY clauses.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Contracts.Sorting;

/// <summary>
/// Canonical sort descriptor representing a single ORDER BY term.
/// </summary>
public sealed record SortDescriptor
{
    /// <summary>Field name (camelCase, matches DTO property).</summary>
    public string Field { get; init; } = string.Empty;

    /// <summary>Sort direction: "asc" (default) or "desc".</summary>
    public string Direction { get; init; } = "asc";

    /// <summary>Whether this sort is descending.</summary>
    public bool IsDescending => string.Equals(Direction, "desc", StringComparison.OrdinalIgnoreCase);

    public static SortDescriptor Ascending(string field)  => new() { Field = field, Direction = "asc" };
    public static SortDescriptor Descending(string field) => new() { Field = field, Direction = "desc" };
}
