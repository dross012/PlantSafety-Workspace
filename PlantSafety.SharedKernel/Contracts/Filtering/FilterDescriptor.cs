// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Contracts
//  Layer:          SharedKernel
//  File:           FilterDescriptor.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Canonical filter descriptor for dynamic query predicates.
//                  Used by the Application query layer to convey caller-supplied
//                  filter criteria without coupling to SQL. Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Contracts.Filtering;

/// <summary>
/// Canonical filter descriptor representing a single field predicate.
/// </summary>
public sealed record FilterDescriptor
{
    /// <summary>Field name (camelCase, matches DTO property).</summary>
    public string Field { get; init; } = string.Empty;

    /// <summary>Comparison operator (eq, neq, gt, gte, lt, lte, contains, startsWith).</summary>
    public string Operator { get; init; } = "eq";

    /// <summary>Value to compare against (always a string; handler converts to target type).</summary>
    public string? Value { get; init; }

    public static FilterDescriptor Eq(string field, string? value)
        => new() { Field = field, Operator = "eq", Value = value };

    public static FilterDescriptor Contains(string field, string? value)
        => new() { Field = field, Operator = "contains", Value = value };
}
