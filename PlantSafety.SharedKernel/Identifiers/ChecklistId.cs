// =====================================================================================================
//  FILE: ChecklistId.cs
//  LAYER: SharedKernel
//  MODULE: Identifiers
//  SCHEMA VERSION: 2026.1
//  AUTHOR: David (Ted) + Copilot
//  CREATED: 2026-05-27
//
//  PURPOSE:
//      Strong identifier for operational checklists. Used across Alerts, Maintenance,
//      Safety, and Workflow modules. Immutable, JSON-safe, deterministic.
//
// =====================================================================================================

using System.Diagnostics.CodeAnalysis;

namespace PlantSafety.SharedKernel.Identifiers;

[JsonConverter(typeof(StrongIdJsonConverter<ChecklistId>))]
public readonly record struct ChecklistId(Guid Value)
    : IStrongId, IParsable<ChecklistId>, ISpanParsable<ChecklistId>
{
    public Guid StrongIdValue => Value;

    public static ChecklistId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();

    public static ChecklistId Parse(string s, IFormatProvider? provider = null)
        => new(Guid.Parse(s));

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out ChecklistId result)
    {
        if (Guid.TryParse(s, out var g))
        {
            result = new ChecklistId(g);
            return true;
        }

        result = default;
        return false;
    }

    public static ChecklistId Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null)
        => new(Guid.Parse(s));

    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        out ChecklistId result)
    {
        if (Guid.TryParse(s, out var g))
        {
            result = new ChecklistId(g);
            return true;
        }

        result = default;
        return false;
    }

    public bool IsEmpty => Value == Guid.Empty;
}
